using Org.BouncyCastle.Crypto.Operators;
using Org.BouncyCastle.X509;
using TestAuthority.Application.Extensions;
using TestAuthority.Domain.Services;

namespace TestAuthority.Application.SignatureFactoryProviders;

/// <summary>
///     Provides a factory that signs crls and certificates using SHA256WithRSA signature algorithm.
/// </summary>
public class GostSignatureFactoryProvider : ISignatureFactoryProvider
{
    private const string SignatureAlgorithm = "Tc26-Gost-3410-12-512-paramSetA";
    private readonly Asn1SignatureFactory signatureFactory;

    public GostSignatureFactoryProvider(ISignerProvider signerProvider, IRandomService randomService)
    {
        var privateKey = signerProvider.GetCertificateSignerInfo().GetSignerPrivateKey();
        signatureFactory = new Asn1SignatureFactory(SignatureAlgorithm, privateKey, randomService.GenerateRandom());
    }

    /// <inheritdoc />
    public X509Crl Generate(X509V2CrlGenerator crlGenerator)
    {
        return crlGenerator.Generate(signatureFactory);
    }

    /// <inheritdoc />
    public X509Certificate Generate(X509V3CertificateGenerator certificateGenerator)
    {
        return certificateGenerator.Generate(signatureFactory);
    }
}
