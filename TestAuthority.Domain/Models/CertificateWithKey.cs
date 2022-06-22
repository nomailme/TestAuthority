using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.X509;

namespace TestAuthority.Domain.Models;

/// <summary>
///     Wrapper for certificate with keys.
/// </summary>
public record CertificateWithKey(X509Certificate Certificate, AsymmetricCipherKeyPair KeyPair)
{
    /// <summary>
    ///     Certificate.
    /// </summary>
    public X509Certificate Certificate { get; } = Certificate;

    /// <summary>
    ///     Key pair.
    /// </summary>
    public AsymmetricCipherKeyPair KeyPair { get; } = KeyPair;
}