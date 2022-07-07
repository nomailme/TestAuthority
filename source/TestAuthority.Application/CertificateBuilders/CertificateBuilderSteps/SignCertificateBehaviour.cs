using MediatR;
using TestAuthority.Application.SignatureFactoryProviders;
using TestAuthority.Domain.Models;

namespace TestAuthority.Application.CertificateBuilders.CertificateBuilderSteps;

public class SignCertificateBehaviour : IPipelineBehavior<CertificateBuilderRequest, CertificateWithKey>
{
    private readonly ISignatureFactoryProvider signatureFactoryProvider;

    public SignCertificateBehaviour(ISignatureFactoryProvider signatureFactoryProvider)
    {
        this.signatureFactoryProvider = signatureFactoryProvider;
    }

    public Task<CertificateWithKey> Handle(CertificateBuilderRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<CertificateWithKey> next)
    {
        request.CertificateGenerator.SetPublicKey(request.KeyPair!.Public);

        var certificate = signatureFactoryProvider.Generate(request.CertificateGenerator);
        certificate.Verify(request.SignerCertificate.GetPublicKey());

        var result = new CertificateWithKey(certificate, request.KeyPair);
        return Task.FromResult(result);
    }
}
