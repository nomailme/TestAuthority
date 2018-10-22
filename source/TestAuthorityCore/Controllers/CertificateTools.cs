using System;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Tls;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Security;

namespace TestAuthorityCore.Controllers
{
    /// <summary>
    /// Provides some useful tools like certificateName conversion.
    /// </summary>
    [Route("api/tools")]
    public class CertificateTools : Controller
    {
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

                string certificateString = ConvertCertificateToPem(certificateEntry);
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
                var key =  store.GetKey(firstAlias);
                var convertedKey = ConvertToPemFormat(key.Key);

                byte[] result = Encoding.ASCII.GetBytes(convertedKey);
                return File(result, MediaTypeNames.Application.Octet, filename);
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

        private static string ConvertCertificateToPem(X509CertificateEntry certificateEntry)
        {
            var generator = new MiscPemGenerator(certificateEntry.Certificate);

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
    }
}
