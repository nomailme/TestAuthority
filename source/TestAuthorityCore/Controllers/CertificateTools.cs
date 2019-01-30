using System;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.X509;

namespace TestAuthorityCore.Controllers
{
    /// <summary>
    /// Provides some useful tools like certificateName conversion.
    /// </summary>
    [Route("api/tools")]
    public class CertificateTools : Controller
    {
        [HttpPost("der-to-pem")]
        public IActionResult ConvertCertificateToPem([FromForm] IFormFile file, string certificateName = "certificate.crt")
        {
            using (var streamReader = new StreamReader(file.OpenReadStream()))
            {
                X509Certificate certificate = new X509CertificateParser().ReadCertificate(streamReader.BaseStream);
                string certificateString = ConvertCertificateToPem(certificate);
                byte[] result = Encoding.ASCII.GetBytes(certificateString);
                return File(result, MediaTypeNames.Application.Octet, certificateName);
            }
        }

        [HttpPost("pem-to-pfx")]
        public IActionResult ConvertToPfx([FromForm] IFormFile pemCertificate, [FromForm] IFormFile pemKey, string password, string filename = "certificate.pfx")
        {
            byte[] certificate;
            byte[] key;

            using (var memoryStream = new MemoryStream())
            {
                pemCertificate.OpenReadStream().CopyTo(memoryStream);
                memoryStream.Position = 0;
                certificate = memoryStream.ToArray();
            }

            using (var memoryStream = new MemoryStream())
            {
                pemKey.OpenReadStream().CopyTo(memoryStream);
                memoryStream.Position = 0;
                key = memoryStream.ToArray();
            }

            byte[] result = ConvertToPfxImpl(certificate, key, password);
            return File(result, MediaTypeNames.Application.Octet, filename);
        }

        [HttpPost("pfx-to-certificate")]
        public IActionResult GetCertificateFromPfx([FromForm] IFormFile file, string password, string certificateName = "certificate.crt")
        {
            using (var streamReader = new StreamReader(file.OpenReadStream()))
            {
                var store = new Pkcs12Store(streamReader.BaseStream, password.ToCharArray());
                string firstAlias = store.Aliases.OfType<string>().FirstOrDefault();
                if (string.IsNullOrWhiteSpace(firstAlias))
                {
                    throw new InvalidOperationException("Unable to find any certificateName in PFX store");
                }

                X509CertificateEntry certificateEntry = store.GetCertificate(firstAlias);

                string certificateString = ConvertCertificateToPem(certificateEntry.Certificate);
                byte[] result = Encoding.ASCII.GetBytes(certificateString);
                return File(result, MediaTypeNames.Application.Octet, certificateName);
            }
        }

        [HttpPost("pfx-to-key")]
        public IActionResult GetKeyFromPfx([FromForm] IFormFile file, string password, string filename = "certificate.key")
        {
            using (var streamReader = new StreamReader(file.OpenReadStream()))
            {
                var store = new Pkcs12Store(streamReader.BaseStream, password.ToCharArray());
                string firstAlias = store.Aliases.OfType<string>().FirstOrDefault();
                if (string.IsNullOrWhiteSpace(firstAlias))
                {
                    throw new InvalidOperationException("Unable to find any certificateName in PFX store");
                }

                X509CertificateEntry certificateEntry = store.GetCertificate(firstAlias);
                AsymmetricKeyEntry key = store.GetKey(firstAlias);
                string convertedKey = ConvertToPemFormat(key.Key);

                byte[] result = Encoding.ASCII.GetBytes(convertedKey);
                return File(result, MediaTypeNames.Application.Octet, filename);
            }
        }

        private static string ConvertCertificateToPem(X509Certificate certificate)
        {
            var generator = new MiscPemGenerator(certificate);

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

        private static string ConvertPrivateKeyToPem(AsymmetricKeyParameter privateKey)
        {
            using (var stringWriter = new StringWriter())
            {
                var pkcsgen = new Pkcs8Generator(privateKey);
                var pemwriter = new PemWriter(stringWriter);
                pemwriter.WriteObject(pkcsgen.Generate());
                return stringWriter.ToString();
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

        private byte[] ConvertToPfxImpl(byte[] certificate, byte[] privateKey, string password)
        {
            AsymmetricCipherKeyPair asymmetricCipherKeyPair = null;
            var certificateEntry = new X509CertificateEntry[1];

            Pkcs12Store store = new Pkcs12StoreBuilder().Build();

            using (var stream = new MemoryStream(privateKey))
            {
                using (var streamReader = new StreamReader(stream))
                {
                    object value = new PemReader(streamReader).ReadObject();
                    if (value is AsymmetricCipherKeyPair pair)
                    {
                        asymmetricCipherKeyPair = pair;
                    }
                }
            }

            using (var stream = new MemoryStream(certificate))
            {
                using (var streamReader = new StreamReader(stream))
                {
                    object value = new PemReader(streamReader).ReadObject();
                    if (value is X509Certificate x509Certificate)
                    {
                        certificateEntry[0] = new X509CertificateEntry(x509Certificate);
                    }
                }
            }

            store.SetKeyEntry("certificate", new AsymmetricKeyEntry(asymmetricCipherKeyPair.Private), certificateEntry);
            var result = new MemoryStream();
            store.Save(result, password.ToCharArray(), new SecureRandom());
            result.Position = 0;
            return result.ToArray();
        }
    }
}
