

using System;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Security;
using TestAuthorityCore.Service;
using TestAuthorityCore.X509;
using X509Certificate = Org.BouncyCastle.X509.X509Certificate;

/// <summary>
/// Service that provides convertion of certificates to <seecref name="CerficateWithKey"/>.
/// </summary>
public class CertificateConverterService : ICertificateConverter
{
    private readonly RandomService randomService;
    private readonly RootCertificateService rootCertificateService;

    /// <summary>
    /// Ctor.
    /// </summary>
    /// <param name="randomService"><seecref name="RandomService"/>.</param>
    /// <param name="rootCertificateService"><seecref name="RootCertificateService"/>.</param>
    public CertificateConverterService(RandomService randomService, RootCertificateService rootCertificateService)
    {
        this.randomService = randomService;
        this.rootCertificateService = rootCertificateService;
    }
    
    /// <summary>
    /// Convert certificate to zip archive with certificate and key in PEM format.
    /// </summary>
    /// <param name="certificate"><seecref name="CerficateWithKey"/>.</param>
    /// <param name="filename">Filename.</param>
    /// <returns></returns>
    public byte[] ConvertToPemArchive(CertificateWithKey certificate, string filename)
    {
        return ConvertToPemArchiveCore(certificate.Certificate, certificate.KeyPair.Private, filename);
    }

    /// <summary>
    /// Convert certificate to pfx file.
    /// </summary>
    /// <param name="certificate"><seecref name="certificate"/></param>
    /// <param name="password">Password for pfx file.</param>
    /// <returns>Pfx file as a byte array.</returns>
    public byte[] ConvertToPfx(CertificateWithKey certificate, string password)
    {

        return ConvertToPfxCore(certificate.Certificate, (RsaPrivateCrtKeyParameters)certificate.KeyPair?.Private, password);
    }

    private byte[] ConvertToPfxCore(X509Certificate2 x509, RsaPrivateCrtKeyParameters rsaParams, string pfxPassword)
    {
        var store = new Pkcs12Store();
        SecureRandom random = randomService.GenerateRandom();
        X509Certificate cert = DotNetUtilities.FromX509Certificate(x509);
        string friendlyName = cert.SubjectDN.ToString();
        var certificateEntry = new X509CertificateEntry(cert);

        store.SetCertificateEntry(friendlyName, certificateEntry);
        store.SetKeyEntry(friendlyName,
            new AsymmetricKeyEntry(rsaParams),
            new[] { certificateEntry });

        using var stream = new MemoryStream();
        store.Save(stream, pfxPassword.ToCharArray(), random);

        return stream.ToArray();
    }

    private byte[] ConvertToPemArchiveCore(X509Certificate2 certificate, AsymmetricKeyParameter keyPair, string filename)
    {
        var rootCertificateWithKey = rootCertificateService.GetRootCertificate();
        var rootCertificate = DotNetUtilities.FromX509Certificate(rootCertificateService.GetRootCertificate()?.Certificate);
        using var stream = new MemoryStream();
        using (var archive = new ZipArchive(stream, ZipArchiveMode.Create, true))
        {
            WriteEntry("root.crt", rootCertificate, archive);
            WriteEntry("private.key", keyPair, archive);
            var cert = DotNetUtilities.FromX509Certificate(certificate);
            WriteEntry("certificate.crt", cert, archive);
        }
        return stream.ToArray();
    }

    private static void WriteEntry(string filename, object entry, ZipArchive archive)
    {
        var entryRecord = archive.CreateEntry(filename);
        using (var entryStream = entryRecord.Open())
        using (var binaryWriter = new BinaryWriter(entryStream))
        {
            var stringRepresentation = ConvertToPemFormat(entry);
            byte[] result = Encoding.ASCII.GetBytes(stringRepresentation);
            binaryWriter.Write(result);
        }
    }

    private static string ConvertToPemFormat(object input)
    {
        var generator = new MiscPemGenerator(input);

        string certificateString;
        using (var textWriter = new StringWriter())
        {
            var writer = new PemWriter(textWriter);
            writer.WriteObject(generator);
            writer.Writer.Flush();
            certificateString = textWriter.ToString();
        }

        if (string.IsNullOrWhiteSpace(certificateString))
        {
            throw new InvalidOperationException();
        }

        return certificateString;
    }
}