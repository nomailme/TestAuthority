using System.Diagnostics.CodeAnalysis;
using System.Text;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.X509;
using TestAuthority.Domain.Models;
using TestAuthority.Domain.Services;
using TestAuthority.Domain.Store;

namespace TestAuthority.Application.Store;

/// <summary>
///     Certificate store. Should be a vault of some sort. But... ehh.
/// </summary>
public class PfxCertificateStore : ICertificateStore
{
    private readonly IRandomService randomService;

    public PfxCertificateStore(IRandomService randomService)
    {
        this.randomService = randomService;
    }

    /// <inheritdoc />
    public bool TryGet(string certificateName, IContainerOptions options, [MaybeNullWhen(false)] out CertificateWithKey certificateWithKey)
    {
        if (Exists(certificateName) == false)
        {
            certificateWithKey = default;
            return false;
        }
        certificateWithKey = GetCertificate(certificateName, options);
        return true;
    }

    /// <inheritdoc />
    public void SaveCertificate(ReadOnlySpan<char> certificateName, CertificateWithKey certificateWithKey, IContainerOptions options)
    {
        if (options is PfxContainerOptions pfxOptions == false)
        {
            throw new ArgumentException("Options must be of type PfxContainerOptions", nameof(options));
        }
        ArgumentNullException.ThrowIfNull(pfxOptions.PfxPassword);

        var path = GetCertificatePath(certificateName);
        var rawData = ConvertToPfx(certificateWithKey.Certificate, (RsaPrivateCrtKeyParameters)certificateWithKey.KeyPair.Private, pfxOptions.PfxPassword);

        File.WriteAllBytes(path, rawData);
    }


    private bool Exists(string certificateName)
    {
        var path = GetCertificatePath(certificateName);
        return File.Exists(path);
    }

    private CertificateWithKey GetCertificate(string certificateName, IContainerOptions options)
    {
        if (options is PfxContainerOptions pfxOptions == false)
        {
            throw new ArgumentException("Options must be of type PfxContainerOptions", nameof(options));
        }
        var path = GetCertificatePath(certificateName);
        var rawData = File.ReadAllBytes(path);
        return Convert(rawData, pfxOptions.PfxPassword);
    }

    private static string GetCertificatePath(ReadOnlySpan<char> certificateName)
    {
        var sb = new StringBuilder();
        sb.Append(certificateName);
        sb.Append(".pfx");
        return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "test-authority", sb.ToString());
    }

    private static CertificateWithKey Convert(byte[] pfxCertificate, ReadOnlySpan<char> password)
    {
        using var stream = new MemoryStream(pfxCertificate);
        var store = new Pkcs12Store();
        store.Load(stream, password.ToArray());
        var alias = store.Aliases.OfType<string>().Single();
        var certificateEntry = store.GetCertificate(alias);
        var keyEntry = store.GetKey(alias);

        var asymmetricCipherKeyPair = new AsymmetricCipherKeyPair(certificateEntry.Certificate.GetPublicKey(), keyEntry.Key);
        var result = new CertificateWithKey(certificateEntry.Certificate, asymmetricCipherKeyPair);
        return result;
    }

    private byte[] ConvertToPfx(X509Certificate cert, AsymmetricKeyParameter rsaParams, string pfxPassword)
    {
        var store = new Pkcs12Store();
        var random = randomService.GenerateRandom();
        var friendlyName = cert.SubjectDN.ToString();
        var certificateEntry = new X509CertificateEntry(cert);

        store.SetCertificateEntry(friendlyName, certificateEntry);
        store.SetKeyEntry(
            friendlyName,
            new AsymmetricKeyEntry(rsaParams),
            new[] { certificateEntry });

        using var stream = new MemoryStream();
        store.Save(stream, pfxPassword.ToCharArray(), random);

        return stream.ToArray();
    }
}
