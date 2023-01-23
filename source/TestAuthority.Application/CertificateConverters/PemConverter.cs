using System.IO.Compression;
using System.Text;
using MediatR;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.X509;
using TestAuthority.Application.CrlBuilders;
using TestAuthority.Domain.Models;
using TestAuthority.Domain.Services;

namespace TestAuthority.Application.CertificateConverters;

public class PemConverter : ICertificateConverter, ICrlConverter
{
    private readonly IMediator mediator;
    private readonly ISignerProvider signerProvider;
    public PemConverter(ISignerProvider signerProvider, IMediator mediator)
    {
        this.signerProvider = signerProvider;
        this.mediator = mediator;
    }

    public async Task<byte[]> Convert(CertificateWithKey input, ICertificateConverterOptions? options = null)
    {
        return await ConvertToPemArchiveCore(input.Certificate, input.KeyPair.Private);
    }

    public Task<byte[]> Convert(CrlFileModel input)
    {
        var pem = ConvertToPemFormat(input);
        return Task.FromResult(Encoding.ASCII.GetBytes(pem));
    }

    private async Task<byte[]> ConvertToPemArchiveCore(X509Certificate certificate, AsymmetricKeyParameter keyPair)
    {
        var signerInfo = signerProvider.GetCertificateSignerInfo();
        var rootCertificate = signerInfo.GetRootCertificate();
        var intermediateCertificates = signerInfo.GetIntermediateCertificates();


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
            outputString = textWriter.ToString().Trim('\r', '\n');
        }

        if (string.IsNullOrWhiteSpace(outputString))
        {
            throw new InvalidOperationException();
        }

        return outputString;
    }

    private static void WriteFullChainEntry(string filename, X509Certificate certificate, List<X509Certificate> intermediate, ZipArchive archive)
    {
        var entryRecord = archive.CreateEntry(filename);
        using var entryStream = entryRecord.Open();
        using var binaryWriter = new BinaryWriter(entryStream);
        var chain = new List<X509Certificate> { certificate };
        chain.AddRange(intermediate);

        var stringBuilder = new StringBuilder();
        foreach (var item in chain)
        {
            var stringRepresentation = ConvertToPemFormat(item);
            stringBuilder.AppendLine(stringRepresentation);
        }

        var result = Encoding.ASCII.GetBytes(stringBuilder.ToString());
        binaryWriter.Write(result);
    }
}
