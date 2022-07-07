using MediatR;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.X509.Extension;
using TestAuthority.Application.Extensions;
using TestAuthority.Application.SignatureFactoryProviders;
using TestAuthority.Domain.Models;

namespace TestAuthority.Application.CrlBuilders.CrlBuilderSteps;

public class GenerateCrlBehaviour : IPipelineBehavior<CrlBuilderRequest, CrlFileModel>
{

    private readonly ITimeServer timeServer;
    private readonly ISignatureFactoryProvider signatureFactoryProvider;

    public GenerateCrlBehaviour(ITimeServer timeServer, ISignatureFactoryProvider signatureFactoryProvider)
    {
        this.timeServer = timeServer;
        this.signatureFactoryProvider = signatureFactoryProvider;
    }

    public Task<CrlFileModel> Handle(CrlBuilderRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<CrlFileModel> next)
    {
        request.CrlGenerator.SetThisUpdate(timeServer.Now.Subtract(TimeSpan.FromHours(5)).DateTime);
        request.CrlGenerator.SetNextUpdate(timeServer.Now.AddYears(1).DateTime);
        request.CrlGenerator.SetIssuerDN(request.SignerInfo.Subject);

        request.CrlGenerator.AddCrlEntry(BigInteger.One, DateTime.Now, CrlReason.PrivilegeWithdrawn);

        request.CrlGenerator.AddExtension(X509Extensions.AuthorityKeyIdentifier,
            false,
            new AuthorityKeyIdentifierStructure(request.SignerInfo.GetSignerCertificate()));

        var crlNumber = new BigInteger(timeServer.Now.ToString("yyyyMMddHHmm"));
        request.CrlGenerator.AddExtension(X509Extensions.CrlNumber, false, new CrlNumber(crlNumber));

        var crlTemp = signatureFactoryProvider.Generate(request.CrlGenerator);
        var result = new CrlFileModel(crlTemp);
        return Task.FromResult(result);
    }
}
