using MediatR;
using Org.BouncyCastle.Asn1.X509;
using TestAuthority.Domain.Models;

namespace TestAuthority.Application.CertificateBuilders.CertificateBuilderSteps;

public class ExtendedKeyUsageExtensionBehaviour : IPipelineBehavior<CertificateBuilderRequest, CertificateWithKey>
{
    public async Task<CertificateWithKey> Handle(CertificateBuilderRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<CertificateWithKey> next)
    {
        var extendedKeyUsage = new ExtendedKeyUsage(KeyPurposeID.IdKPClientAuth, KeyPurposeID.IdKPServerAuth);
        request.CertificateGenerator.AddExtension(X509Extensions.ExtendedKeyUsage.Id, false, extendedKeyUsage);
        return await next();
    }
}
