using MediatR;
using Org.BouncyCastle.Asn1.X509;
using TestAuthority.Domain.Models;

namespace TestAuthority.Application.CertificateBuilders.CertificateBuilderSteps;

public class BasicConstraintsExtensionBehaviour : IPipelineBehavior<CertificateBuilderRequest, CertificateWithKey>
{
    public async Task<CertificateWithKey> Handle(CertificateBuilderRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<CertificateWithKey> next)
    {
        if (request.IsCaCertificate)
        {
            request.CertificateGenerator.AddExtension(X509Extensions.BasicConstraints, true, new BasicConstraints(true));
            return await next();
        }

        request.CertificateGenerator.AddExtension(X509Extensions.BasicConstraints, true, new BasicConstraints(false));
        return await next();
    }
}
