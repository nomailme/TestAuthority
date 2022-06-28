using MediatR;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Operators;
using TestAuthority.Domain.Models;
using TestAuthority.Domain.Services;

namespace TestAuthority.Application.CertificateBuilders.CertificateBuilderSteps;

public class SignCertificateBehaviour : IPipelineBehavior<CertificateBuilderRequest, CertificateWithKey>
{
    private const string SignatureAlgorithm = "SHA256WithRSA";
    private readonly IRandomService randomService;

    public SignCertificateBehaviour(IRandomService randomService)
    {
        this.randomService = randomService;
    }

    public Task<CertificateWithKey> Handle(CertificateBuilderRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<CertificateWithKey> next)
    {
        ISignatureFactory signatureFactory = new Asn1SignatureFactory(SignatureAlgorithm, request.SignerInfo.SignerCertificate.KeyPair.Private, randomService.GenerateRandom());
        request.CertificateGenerator.SetPublicKey(request.KeyPair!.Public);

        var certificate = request.CertificateGenerator.Generate(signatureFactory);
        certificate.Verify(request.SignerInfo.SignerCertificate.KeyPair.Public);

        var result = new CertificateWithKey(certificate, request.KeyPair);
        return Task.FromResult(result);
    }
}
