using MediatR;
using TestAuthority.Domain;
using TestAuthority.Domain.Models;

namespace TestAuthority.Application.CertificateBuilders.CertificateBuilderSteps;

public class CertificateValidityBehaviour : IPipelineBehavior<CertificateBuilderRequest, CertificateWithKey>
{
    private readonly ITimeServer timeServer;

    public CertificateValidityBehaviour(ITimeServer timeServer)
    {
        this.timeServer = timeServer;
    }

    public async Task<CertificateWithKey> Handle(CertificateBuilderRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<CertificateWithKey> next)
    {
        var notBefore = timeServer.Now.Subtract(TimeSpan.FromHours(5)).UtcDateTime;
        var notAfter = timeServer.Now.Add(TimeSpan.FromDays(request.RequestModel.ValidityInDays)).UtcDateTime;
        request.CertificateGenerator.SetNotBefore(notBefore);
        request.CertificateGenerator.SetNotAfter(notAfter);
        return await next();
    }
}
