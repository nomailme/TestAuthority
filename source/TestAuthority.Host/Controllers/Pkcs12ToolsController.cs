using System.IO;
using System.Net.Mime;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.X509;

namespace TestAuthority.Host.Controllers;

/// <summary>
///     Provides API for pfx tooling.
/// </summary>
[Route("api/pkcs12")]
public class Pkcs12ToolsController : Controller
{
    /// <summary>
    ///     Convert certificate and key in Pem format to Pfx(Pkcs12).
    /// </summary>
    /// <param name="pemCertificate">Certificate in Pem format.</param>
    /// <param name="pemKey">Private key in Pem format.</param>
    /// <param name="password">Password for the private key.</param>
    /// <param name="filename">Output filename.</param>
    /// <returns>Certificate with private key in Pkcs12 container.</returns>
    [HttpPost("from-pem")]
    public IActionResult ConvertToPfx(IFormFile pemCertificate, IFormFile pemKey, string password,
        string filename = "certificate.pfx")
    {
        var certificate = ToArray(pemCertificate.OpenReadStream());
        var key = ToArray(pemKey.OpenReadStream());

        var result = ConvertToPfxImpl(certificate, key, password);
        return File(result, MediaTypeNames.Application.Octet, filename);
    }

    private static byte[] ToArray(Stream stream)
    {
        using var memoryStream = new MemoryStream();
        stream.CopyTo(memoryStream);
        memoryStream.Position = 0;
        return memoryStream.ToArray();
    }

    private static TOutput ToCrypto<TOutput>(byte[] input)
        where TOutput : class
    {
        using var stream = new MemoryStream(input);
        using var streamReader = new StreamReader(stream);
        var value = new PemReader(streamReader).ReadObject();
        if (value is TOutput result) return result;

        return null;
    }

    private static byte[] ConvertToPfxImpl(byte[] certificate, byte[] privateKey, string password)
    {
        var store = new Pkcs12StoreBuilder().Build();

        var certificateEntry = new X509CertificateEntry[1];
        var x509Certificate = ToCrypto<X509Certificate>(certificate);
        certificateEntry[0] = new X509CertificateEntry(x509Certificate);

        var asymmetricCipherKeyPair = ToCrypto<AsymmetricCipherKeyPair>(privateKey);

        store.SetKeyEntry(x509Certificate.SubjectDN.ToString(),
            new AsymmetricKeyEntry(asymmetricCipherKeyPair.Private), certificateEntry);
        var result = new MemoryStream();
        store.Save(result, password.ToCharArray(), new SecureRandom());
        result.Position = 0;
        return result.ToArray();
    }
}
