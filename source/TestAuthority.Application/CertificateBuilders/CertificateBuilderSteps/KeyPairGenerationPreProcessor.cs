using MediatR.Pipeline;
using TestAuthority.Domain.Services;

namespace TestAuthority.Application.CertificateBuilders.CertificateBuilderSteps;

public class KeyPairGenerationBehaviour : IRequestPreProcessor<CertificateBuilderRequest>
{
    private readonly IRandomService randomService;

    public KeyPairGenerationBehaviour(IRandomService randomService)
    {
        this.randomService = randomService;
    }

    public Task Process(CertificateBuilderRequest request, CancellationToken cancellationToken)
    {
        var keyPair = CertificateBuilder2.GenerateKeyPair(2048, randomService.GenerateRandom());

        request.CertificateGenerator.SetPublicKey(keyPair.Public);
        request.KeyPair = keyPair;
        return Task.CompletedTask;
    }
}
