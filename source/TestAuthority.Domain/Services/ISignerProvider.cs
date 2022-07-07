using TestAuthority.Domain.Models;

namespace TestAuthority.Domain.Services;

public interface ISignerProvider
{
    /// <summary>
    ///     Get root certificate.
    /// </summary>
    /// <returns>Root certificate.</returns>
    CertificateSignerInfo GetCertificateSignerInfo();
}
