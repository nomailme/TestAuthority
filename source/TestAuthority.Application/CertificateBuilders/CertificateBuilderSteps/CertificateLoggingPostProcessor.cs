using System.Collections;
using System.Text;
using MediatR.Pipeline;
using Microsoft.Extensions.Logging;
using TestAuthority.Domain.Models;

namespace TestAuthority.Application.CertificateBuilders.CertificateBuilderSteps;

public class CertificateLoggingPostProcessor: IRequestPostProcessor<CertificateBuilderRequest, CertificateWithKey>
{
    private readonly ILogger<CertificateLoggingPostProcessor> logger;

    public CertificateLoggingPostProcessor(ILogger<CertificateLoggingPostProcessor> logger)
    {
        this.logger = logger;
    }

    public Task Process(CertificateBuilderRequest request, CertificateWithKey response, CancellationToken cancellationToken)
    {
        var sb = new StringBuilder();
        var certificate = response.Certificate;

        sb.AppendLine("== Certificate Info ==");
        sb.AppendLine($"Subject:      {certificate.SubjectDN.ToString()}");
        sb.AppendLine($"Issuer:       {certificate.IssuerDN.ToString()}");
        sb.AppendLine($"Not Before:   {certificate.NotBefore.ToString("g")}");
        sb.AppendLine($"Not After:    {certificate.NotAfter.ToString("g")}");
        sb.AppendLine($"Serial Number {certificate.SerialNumber}");
        sb.AppendLine("Extensions:");
        sb.AppendLine("  Subject Alternative Name");
        foreach (ArrayList subjectAlternativeName in certificate.GetSubjectAlternativeNames())
        {
            sb.AppendLine($"    - {subjectAlternativeName[1]}");
        }
        logger.LogInformation("{CertificateInformation}",sb.ToString());
        return Task.CompletedTask;
    }
}
