using Org.BouncyCastle.X509;

namespace TestAuthority.Domain.Services;

/// <summary>
///     Crl builder interface.
/// </summary>
public interface ICrlBuilder
{
    /// <summary>
    ///     Generate a dummy CRL file.
    /// </summary>
    /// <returns>Crl.</returns>
    X509Crl Generate();
}