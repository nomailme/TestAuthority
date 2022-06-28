using System;
using System.IO;
using System.Linq;
using System.Net.Mime;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Pkcs;

namespace TestAuthority.Host.Controllers;

/// <summary>
/// Tools to work with Pem certificates.
/// </summary>
public class PemToolsController : Controller
{
    /// <summary>
    /// Convert pfx to pem certificate.
    /// </summary>
    /// <param name="request">Request.</param>
    /// <param name="password">Pfx password.</param>
    /// <param name="certificateName">Name of the output certificate.</param>
    /// <returns>Certificate.</returns>
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

    /// <summary>
    /// Convert pfx to pem key.
    /// </summary>
    /// <param name="request">Request.</param>
    /// <param name="password">Pfx password.</param>
    /// <param name="filename">Name of the output key.</param>
    /// <returns>Key.</returns>
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
