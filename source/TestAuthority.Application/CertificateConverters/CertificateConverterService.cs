using System.IO.Compression;
using System.Text;
using MediatR;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.X509;
using TestAuthority.Application.CrlBuilders;
using TestAuthority.Application.Extensions;
using TestAuthority.Domain.CertificateConverters;
using TestAuthority.Domain.Models;
using TestAuthority.Domain.Services;

namespace TestAuthority.Application.CertificateConverters;

/// <summary>
///     Service that provides conversion of certificates to <seecref name="CerficateWithKey" />.
/// </summary>
public class CertificateConverterService : ICertificateConverter
{
    private readonly IMediator mediator;
    private readonly IRandomService randomService;
    private readonly ISignerProvider signerProvider;

    /// <summary>
    ///     Ctor.
    /// </summary>
    /// <param name="randomService"><seecref name="RandomService" />.</param>
    /// <param name="signerProvider"><see cref="ISignerProvider" />.</param>
    /// <param name="mediator"><see cref="IMediator" />.</param>
    public CertificateConverterService(IRandomService randomService, ISignerProvider signerProvider, IMediator mediator)
    {
        this.randomService = randomService;
        this.signerProvider = signerProvider;
        this.mediator = mediator;
    }

    /// <summary>
    ///     Convert certificate to zip archive with certificate and key in PEM format.
    /// </summary>
    /// <param name="certificate"><seecref name="CerficateWithKey" />.</param>
    /// <returns></returns>
    public async Task<byte[]> ConvertToPemArchiveAsync(CertificateWithKey certificate)
    {
        return await ConvertToPemArchiveCore(certificate.Certificate, certificate.KeyPair.Private);
    }

    /// <summary>
    ///     Convert certificate to pfx file.
    /// </summary>
    /// <param name="certificate">
    ///     <seecref name="certificate" />
    /// </param>
    /// <param name="password">Password for pfx file.</param>
    /// <returns>Pfx file as a byte array.</returns>
    public byte[] ConvertToPfx(CertificateWithKey certificate, string password)
    {
        return ConvertToPfxCore(certificate.Certificate, (RsaPrivateCrtKeyParameters)certificate.KeyPair.Private, password);
    }

    /// <inheritdoc />
    public byte[] ConvertToPem(CrlFileModel crl)
    {
        var pem = ConvertToPemFormat(crl.Crl);
        return Encoding.ASCII.GetBytes(pem);
    }

    private byte[] ConvertToPfxCore(X509Certificate certificate, AsymmetricKeyParameter rsaParams, string pfxPassword)
    {
        var store = new Pkcs12Store();
        var random = randomService.GenerateRandom();
        var friendlyName = certificate.SubjectDN.ToString();
        var certificateEntry = new X509CertificateEntry(certificate);

        store.SetCertificateEntry(friendlyName, certificateEntry);
        store.SetKeyEntry(friendlyName,
            new AsymmetricKeyEntry(rsaParams),
            new[] { certificateEntry });

        using var stream = new MemoryStream();
        store.Save(stream, pfxPassword.ToCharArray(), random);

        return stream.ToArray();
    }

    private async Task<byte[]> ConvertToPemArchiveCore(X509Certificate certificate, AsymmetricKeyParameter keyPair)
    {
        var signerInfo = signerProvider.GetCertificateSignerInfo();
        var rootCertificate = signerInfo.GetRootCertificate();
        var intermediateCertificates = signerInfo.GetIntermediateCertificates().Select(x => x.Certificate).ToList();


        var crls = await GetCrls(signerInfo);


        using var stream = new MemoryStream();
        using (var archive = new ZipArchive(stream, ZipArchiveMode.Create, true))
        {
            WriteEntry("root.crt", rootCertificate, archive);
            WriteEntry("private.key", keyPair, archive);
            WriteEntry("certificate.crt", certificate, archive);
            for (var index = 0; index < crls.Count; index++)
            {
                var crl = crls[index];
                WriteEntry($"crl_{index}.crl", crl.Crl, archive);
            }
            for (var index = 0; index < intermediateCertificates.Count; index++)
            {
                var intermediate = intermediateCertificates[index];
                WriteEntry($"intermediate_{index + 1}.crt", intermediate, archive);
            }
            WriteFullChainEntry("fullchain.crt", certificate, intermediateCertificates, archive);
        }
        return stream.ToArray();
    }

    private async Task<List<CrlFileModel>> GetCrls(CertificateSignerInfo signerInfo)
    {
        var serialNumberRequests = signerInfo.CertificateChain
            .Select(x => x.Certificate)
            .Select(x => x.SerialNumber.ToString(16))
            .Select(x => new CrlBuilderRequest(signerInfo, x));

        var result = new List<CrlFileModel>();
        foreach (var request in serialNumberRequests)
        {
            var crl = await mediator.Send(request);
            result.Add(crl);
        }
        return result;
    }

    private static void WriteFullChainEntry(string filename, X509Certificate certificate, List<X509Certificate> intermediate, ZipArchive archive)
    {
        var entryRecord = archive.CreateEntry(filename);
        using var entryStream = entryRecord.Open();
        using var binaryWriter = new BinaryWriter(entryStream);
        var fullchain = new List<X509Certificate> { certificate };
        fullchain.AddRange(intermediate);

        var stringBuilder = new StringBuilder();
        foreach (var item in fullchain)
        {
            var stringRepresentation = ConvertToPemFormat(item);
            stringBuilder.AppendLine(stringRepresentation);
        }

        var result = Encoding.ASCII.GetBytes(stringBuilder.ToString());
        binaryWriter.Write(result);
    }

    private static void WriteEntry(string filename, object entry, ZipArchive archive)
    {
        var entryRecord = archive.CreateEntry(filename);
        using var entryStream = entryRecord.Open();
        using var binaryWriter = new BinaryWriter(entryStream);
        var stringRepresentation = ConvertToPemFormat(entry);
        var result = Encoding.ASCII.GetBytes(stringRepresentation);
        binaryWriter.Write(result);
    }

    private static string ConvertToPemFormat(object input)
    {
        var generator = new MiscPemGenerator(input);

        string outputString;
        using (var textWriter = new StringWriter())
        {
            var writer = new PemWriter(textWriter);
            writer.WriteObject(generator);
            writer.Writer.Flush();
            outputString = textWriter.ToString();
        }

        if (string.IsNullOrWhiteSpace(outputString))
        {
            throw new InvalidOperationException();
        }

        return outputString;
    }
}
