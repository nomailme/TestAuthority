using MediatR;
using TestAuthority.Domain.Models;

namespace TestAuthority.Application.CertificateBuilders.CertificateBuilderSteps;

public class IssuerNameBehaviour : IPipelineBehavior<CertificateBuilderRequest, CertificateWithKey>
{
    public Task<CertificateWithKey> Handle(CertificateBuilderRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<CertificateWithKey> next)
    {
        request.CertificateGenerator.SetIssuerDN(request.SignerInfo.Subject);
        return next();
    }
}
