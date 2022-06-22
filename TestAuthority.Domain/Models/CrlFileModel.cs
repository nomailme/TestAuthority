using Org.BouncyCastle.X509;

namespace TestAuthority.Domain.Models;

/// <summary>
///     Crl wrapper.
/// </summary>
public record CrlFileModel(X509Crl Crl)
{
    /// <summary>
    ///     Crl file.
    /// </summary>
    public X509Crl Crl { get; set; } = Crl;
}