namespace TestAuthority.Domain.Models;

/// <summary>
///     Request for certificate.
/// </summary>
public record CertificateRequestModel
{
    /// <summary>
    ///     Common name of the certificate.
    /// </summary>
    public string? CommonName { get; init; }

    /// <summary>
    ///     List of domain names to be included in Subject Alternative Name extension
    /// </summary>
    public List<string> Hostnames { get; init; } = new();

    /// <summary>
    ///     List of IP addresses to be included in Subject Alternative Name extension
    /// </summary>
    public List<string> IpAddresses { get; init; } = new();

    /// <summary>
    ///     Certificate validity in days.
    /// </summary>
    /// <value>Default value of 364.</value>
    public int ValidityInDays { get; init; } = 364;
}
