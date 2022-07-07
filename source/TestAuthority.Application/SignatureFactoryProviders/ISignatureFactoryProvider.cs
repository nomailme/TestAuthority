using Org.BouncyCastle.X509;

namespace TestAuthority.Application.SignatureFactoryProviders;

/// <summary>
///     Signature factory provider.
/// </summary>
public interface ISignatureFactoryProvider
{
    /// <summary>
    ///     Generate Crl.
    /// </summary>
    /// <param name="crlGenerator"><see cref="X509V2CrlGenerator" />.</param>
    /// <returns><see cref="X509Crl" />.</returns>
    X509Crl Generate(X509V2CrlGenerator crlGenerator);

    /// <summary>
    ///     Generate certificate.
    /// </summary>
    /// <param name="certificateGenerator"><see cref="X509V3CertificateGenerator" />.</param>
    /// <returns><see cref="X509Certificate" />.</returns>
    X509Certificate Generate(X509V3CertificateGenerator certificateGenerator);
}
