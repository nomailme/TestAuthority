using MediatR;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.X509;
using TestAuthority.Domain.Models;

namespace TestAuthority.Application.CertificateBuilders.CertificateBuilderSteps;

public class AuthorityKeyIdentifierExtensionBehavior : IPipelineBehavior<CertificateBuilderRequest, CertificateWithKey>
{
    public async Task<CertificateWithKey> Handle(CertificateBuilderRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<CertificateWithKey> next)
    {
        var subjectPublicKeyInfo = SubjectPublicKeyInfoFactory.CreateSubjectPublicKeyInfo(request.SignerInfo.SignerCertificate.KeyPair.Public);
        request.CertificateGenerator.AddExtension(X509Extensions.AuthorityKeyIdentifier.Id, false, new AuthorityKeyIdentifier(subjectPublicKeyInfo));
        return await next();
    }
}
