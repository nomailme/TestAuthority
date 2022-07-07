using MediatR;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.X509;
using TestAuthority.Domain.Models;

namespace TestAuthority.Application.CertificateBuilders;

/// <summary>
///     What an awful naming. Please do something about it. //TODO: Do it.
/// </summary>
public record CertificateBuilderRequest(CertificateRequestModel RequestModel, X509Certificate SignerCertificate, bool IsCaCertificate = false) : IRequest<CertificateWithKey>
{
    public X509V3CertificateGenerator CertificateGenerator { get; } = new();

    public AsymmetricCipherKeyPair? KeyPair { get; set; }
}
