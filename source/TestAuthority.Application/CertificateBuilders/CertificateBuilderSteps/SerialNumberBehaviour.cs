using MediatR;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Utilities;
using TestAuthority.Domain.Models;
using TestAuthority.Domain.Services;

namespace TestAuthority.Application.CertificateBuilders.CertificateBuilderSteps;

public class SerialNumberBehaviour : IPipelineBehavior<CertificateBuilderRequest, CertificateWithKey>
{
    private readonly IRandomService randomService;

    public SerialNumberBehaviour(IRandomService randomService)
    {
        this.randomService = randomService;
    }

    public async Task<CertificateWithKey> Handle(CertificateBuilderRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<CertificateWithKey> next)
    {
        var serialNumber =
            BigIntegers.CreateRandomInRange(BigInteger.One, BigInteger.ValueOf(long.MaxValue), randomService.GenerateRandom());
        request.CertificateGenerator.SetSerialNumber(serialNumber);
        return await next();
    }
}
