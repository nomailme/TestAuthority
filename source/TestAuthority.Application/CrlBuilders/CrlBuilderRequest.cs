using MediatR;
using Org.BouncyCastle.X509;
using TestAuthority.Domain.Models;

namespace TestAuthority.Application.CrlBuilders;

/// <summary>
///     What an awful naming. Please do something about it. //TODO: Do it.
/// </summary>
public record CrlBuilderRequest(CertificateSignerInfo SignerInfo) : IRequest<CrlFileModel>
{
    public CertificateSignerInfo SignerInfo { get; } = SignerInfo;

    public X509V2CrlGenerator CrlGenerator { get; } = new();
}
