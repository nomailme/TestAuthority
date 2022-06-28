using MediatR;
using Org.BouncyCastle.Asn1.X509;
using TestAuthority.Domain.Models;

namespace TestAuthority.Application.CertificateBuilders.CertificateBuilderSteps;

public class CommonNameBehaviour : IPipelineBehavior<CertificateBuilderRequest, CertificateWithKey>
{
    public async Task<CertificateWithKey> Handle(CertificateBuilderRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<CertificateWithKey> next)
    {
        request.CertificateGenerator.SetSubjectDN(new X509Name($"CN={request.RequestModel.CommonName}"));
        return await next();
    }
}
