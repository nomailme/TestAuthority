namespace TestAuthority.Domain.Models;

/// <summary>
///     Represent certificate output format
/// </summary>
public enum OutputFormat
{
    /// <summary>
    ///     Pfx file.
    /// </summary>
    Pfx,

    /// <summary>
    ///     Zip archive with certificate and key in PEM format.
    /// </summary>
    Pem
}