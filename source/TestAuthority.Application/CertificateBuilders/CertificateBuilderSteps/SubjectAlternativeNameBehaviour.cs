using MediatR;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.X509;
using TestAuthority.Domain.Models;

namespace TestAuthority.Application.CertificateBuilders.CertificateBuilderSteps;

public class SubjectAlternativeNameBehaviour : IPipelineBehavior<CertificateBuilderRequest, CertificateWithKey>
{
    public async Task<CertificateWithKey> Handle(CertificateBuilderRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<CertificateWithKey> next)
    {
        var result = new List<Asn1Encodable>();

        request.RequestModel.Hostnames.Select(x => new GeneralName(GeneralName.DnsName, x))
            .Select(x => x as Asn1Encodable)
            .ToList()
            .ForEach(result.Add);

        request.RequestModel.IpAddresses.Select(x => new GeneralName(GeneralName.IPAddress, x))
            .Select(x => x as Asn1Encodable)
            .ToList()
            .ForEach(result.Add);

        var extension = new DerSequence(result.ToArray());
        request.CertificateGenerator.AddExtension(X509Extensions.SubjectAlternativeName.Id, false, extension);
        return await next();
    }
}
