using FluentValidation;
using TestAuthority.Domain.Models;

namespace TestAuthority.Host.Validators;

/// <summary>
///     Validation for certificate request.
/// </summary>
public class CertificateRequestValidator : AbstractValidator<Contracts.CertificateRequestModel>
{
    /// <summary>
    ///     Ctor.
    /// </summary>
    public CertificateRequestValidator()
    {
        RuleFor(x => x.CommonName)
            .NotEmpty().WithMessage("You must provide a Common Name");
        RuleFor(x => x)
            .Must(AnyHostnamesOrIpAddresses).WithMessage("You must provide at least one hostname or IP address.");
        RuleFor(x => x.Password)
            .MinimumLength(1).When(x => x.Format == OutputFormat.Pfx)
            .WithMessage("You must provide a password for PFX");
    }

    private static bool AnyHostnamesOrIpAddresses(Contracts.CertificateRequestModel request)
    {
        var sanRecords = 0;
        sanRecords += request.Hostname?.Length ?? 0;
        sanRecords += request.IpAddress?.Length ?? 0;
        return sanRecords > 0;
    }
}
