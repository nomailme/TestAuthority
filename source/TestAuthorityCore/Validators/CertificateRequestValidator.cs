using FluentValidation;
using TestAuthorityCore.Contracts;
using TestAuthorityCore.X509;

namespace TestAuthorityCore.Validators
{
    /// <summary>
    ///     Validation for certificate request.
    /// </summary>
    public class CertificateRequestValidator : AbstractValidator<CertificateRequestModel>
    {
        /// <summary>
        ///     Ctor.
        /// </summary>
        public CertificateRequestValidator()
        {
            RuleFor(x => x).Must(AnyHostnamesOrIpAddresses)
                .WithMessage("You must provide at least one hostname or IP address.");
            RuleFor(x => x.Password).MinimumLength(1).When(x => x.Format == CertificateFormat.Pfx)
                .WithMessage("You must provide a password for PFX");
        }

        private bool AnyHostnamesOrIpAddresses(CertificateRequestModel request)
        {
            var sanRecords = 0;
            sanRecords += request.Hostname?.Length ?? 0;
            sanRecords += request.IpAddress?.Length ?? 0;
            return sanRecords > 0;
        }
    }
}