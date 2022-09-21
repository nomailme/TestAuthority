using MediatR;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.X509.Extension;
using TestAuthority.Application.SignatureFactoryProviders;
using TestAuthority.Domain;
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
        var signerInfo = request.SerialNumber == String.Empty
            ? request.SignerInfo.CertificateChain.Last().Certificate
            : request.SignerInfo.CertificateChain.First(x => string.Equals(x.Certificate.SerialNumber.ToString(16), request.SerialNumber)).Certificate;

        request.CrlGenerator.SetThisUpdate(timeServer.Now.Subtract(TimeSpan.FromHours(5)).DateTime);
        request.CrlGenerator.SetNextUpdate(timeServer.Now.AddYears(1).DateTime);
        request.CrlGenerator.SetIssuerDN(signerInfo.SubjectDN);

        request.CrlGenerator.AddCrlEntry(BigInteger.One, DateTime.Now, CrlReason.KeyCompromise);

        request.CrlGenerator.AddExtension(X509Extensions.AuthorityKeyIdentifier,
            false,
            new AuthorityKeyIdentifierStructure(signerInfo.GetPublicKey()));

        var crlNumber = new BigInteger(timeServer.Now.ToString("yyyyMMddHHmm"));
        request.CrlGenerator.AddExtension(X509Extensions.CrlNumber, false, new CrlNumber(crlNumber));

        var crl = signatureFactoryProvider.Generate(request.CrlGenerator);
        var result = new CrlFileModel(crl);
        return Task.FromResult(result);
    }
}
