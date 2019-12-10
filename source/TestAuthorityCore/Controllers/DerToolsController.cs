using System;
using System.IO;
using System.Net.Mime;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.X509;

namespace TestAuthorityCore.Controllers
{
    /// <summary>
    /// Provides some useful tools like certificateName conversion.
    /// </summary>
    [Route("api/tools")]
    public class DerToolsController : Controller
    {
        [HttpPost("der-to-pem")]
        public IActionResult ConvertCertificateToPem(IFormFile request, string certificateName = "certificate.crt")
        {
            using var streamReader = new StreamReader(request.OpenReadStream());
            X509Certificate certificate = new X509CertificateParser().ReadCertificate(streamReader.BaseStream);
            string certificateString = ConvertCertificateToPem(certificate);
            byte[] result = Encoding.ASCII.GetBytes(certificateString);
            return File(result, MediaTypeNames.Application.Octet, certificateName);
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
    }
}
