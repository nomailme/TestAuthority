using System;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Pkcs;

namespace TestAuthorityCore.Controllers
{
    public class PemToolsController : Controller
    {
        [HttpPost("pfx-to-certificate")]
        public IActionResult GetCertificateFromPfx(IFormFile request, string password, string certificateName = "certificate.crt")
        {
            using var streamReader = new StreamReader(request.OpenReadStream());
            var store = new Pkcs12Store(streamReader.BaseStream, password.ToCharArray());
            string firstAlias = store.Aliases.OfType<string>().FirstOrDefault();
            if (string.IsNullOrWhiteSpace(firstAlias))
            {
                throw new InvalidOperationException("Unable to find any certificateName in PFX store");
            }

            X509CertificateEntry certificateEntry = store.GetCertificate(firstAlias);

            string certificateString = ConvertToPemFormat(certificateEntry.Certificate);
            byte[] result = Encoding.ASCII.GetBytes(certificateString);
            return File(result, MediaTypeNames.Application.Octet, certificateName);
        }

        [HttpPost("pfx-to-key")]
        public IActionResult GetKeyFromPfx(IFormFile request, string password, string filename = "certificate.key")
        {
            using var streamReader = new StreamReader(request.OpenReadStream());
            var store = new Pkcs12Store(streamReader.BaseStream, password.ToCharArray());
            string firstAlias = store.Aliases.OfType<string>().FirstOrDefault();
            if (string.IsNullOrWhiteSpace(firstAlias))
            {
                throw new InvalidOperationException("Unable to find any certificateName in PFX store");
            }

            AsymmetricKeyEntry key = store.GetKey(firstAlias);
            string convertedKey = ConvertToPemFormat(key.Key);

            byte[] result = Encoding.ASCII.GetBytes(convertedKey);
            return File(result, MediaTypeNames.Application.Octet, filename);
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
}
