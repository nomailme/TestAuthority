using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Security;
using TestAuthority.Application;
using TestAuthority.Application.Random;
using TestAuthority.Domain.Models;
using X509Certificate = Org.BouncyCastle.X509.X509Certificate;

namespace TestAuthority.Host.Service
{
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
            return ConvertToPemArchiveCore(certificate.Certificate, certificate.KeyPair.Private);
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

        /// <inheritdoc />
        public byte[] ConvertToPem(CrlFileModel crl)
        {
            var pem = ConvertToPemFormat(crl.Crl);
            return Encoding.ASCII.GetBytes(pem);
        }

        private byte[] ConvertToPfxCore(X509Certificate certificate, RsaPrivateCrtKeyParameters rsaParams, string pfxPassword)
        {
            var store = new Pkcs12Store();
            SecureRandom random = randomService.GenerateRandom();
            string friendlyName = certificate.SubjectDN.ToString();
            var certificateEntry = new X509CertificateEntry(certificate);

            store.SetCertificateEntry(friendlyName, certificateEntry);
            store.SetKeyEntry(friendlyName,
                new AsymmetricKeyEntry(rsaParams),
                new[] { certificateEntry });

            using var stream = new MemoryStream();
            store.Save(stream, pfxPassword.ToCharArray(), random);

            return stream.ToArray();
        }

        private byte[] ConvertToPemArchiveCore(X509Certificate certificate, AsymmetricKeyParameter keyPair)
        {
            var rootCertificateWithKey = rootCertificateService.GetRootCertificate();
            var rootCertificate = rootCertificateWithKey.Certificate;
            using var stream = new MemoryStream();
            using (var archive = new ZipArchive(stream, ZipArchiveMode.Create, true))
            {
                WriteEntry("root.crt", rootCertificate, archive);
                WriteEntry("private.key", keyPair, archive);
                WriteEntry("certificate.crt", certificate, archive);
            }
            return stream.ToArray();
        }

        private static void WriteEntry(string filename, object entry, ZipArchive archive)
        {
            var entryRecord = archive.CreateEntry(filename);
            using var entryStream = entryRecord.Open();
            using var binaryWriter = new BinaryWriter(entryStream);
            var stringRepresentation = ConvertToPemFormat(entry);
            byte[] result = Encoding.ASCII.GetBytes(stringRepresentation);
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
}