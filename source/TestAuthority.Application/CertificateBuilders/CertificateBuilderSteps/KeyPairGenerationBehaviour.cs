using MediatR;
using TestAuthority.Domain.Models;
using TestAuthority.Domain.Services;

namespace TestAuthority.Application.CertificateBuilders.CertificateBuilderSteps;

public class KeyPairGenerationBehaviour : IPipelineBehavior<CertificateBuilderRequest, CertificateWithKey>
{
    private readonly IRandomService randomService;

    public KeyPairGenerationBehaviour(IRandomService randomService)
    {
        this.randomService = randomService;
    }

    public async Task<CertificateWithKey> Handle(CertificateBuilderRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<CertificateWithKey> next)
    {
        var keyPair = CertificateBuilder2.GenerateKeyPair(2048, randomService.GenerateRandom());

        request.CertificateGenerator.SetPublicKey(keyPair.Public);
        request.KeyPair = keyPair;
        return await next();
    }
}
