using MediatR;
using Org.BouncyCastle.X509;
using TestAuthority.Domain.Models;

namespace TestAuthority.Application.CrlBuilders;

/// <summary>
///     What an awful naming. Please do something about it. //TODO: Do it.
/// </summary>
public record CrlBuilderRequest : IRequest<CrlFileModel>
{
    public CrlBuilderRequest(CertificateSignerInfo signerInfo)
    {
        SignerInfo = signerInfo;
        SerialNumber = signerInfo.CertificateChain.Last().Certificate.SerialNumber.ToString(16);
    }

    public CrlBuilderRequest(CertificateSignerInfo signerInfo, string serialNumber)
    {
        SignerInfo = signerInfo;
        SerialNumber = serialNumber;
    }

    public CertificateSignerInfo SignerInfo { get; }

    public X509V2CrlGenerator CrlGenerator { get; } = new();

    public string SerialNumber { get; } = String.Empty;
}
