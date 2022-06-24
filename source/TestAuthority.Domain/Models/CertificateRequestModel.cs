namespace TestAuthority.Domain.Models;

/// <summary>
///     Request for certificate.
/// </summary>
public record CertificateRequestModel
{
    /// <summary>
    ///     Common name of the certificate.
    /// </summary>
    public string? CommonName { get; set; }

    /// <summary>
    ///     List of domain names to be included in Subject Alternative Name extension
    /// </summary>
    public List<string> Hostnames { get; set; } = new();

    /// <summary>
    ///     List of IP addresses to be included in Subject Alternative Name extension
    /// </summary>
    public List<string> IpAddresses { get; set; } = new();

    /// <summary>
    ///     Certificate validtiy in days.
    /// </summary>
    /// <value>Default value of 364.</value>
    public int ValidityInDays { get; set; } = 364;
}