using TestAuthority.Domain.Models;

namespace TestAuthority.Domain.CertificateConverters;

/// <summary>
/// Provides methods to convert certificates and keys.
/// </summary>
public interface ICertificateConverter
{
    /// <summary>
    /// Convert certificate with key to ZIP archive containing certificate and key in PEM format.
    /// </summary>
    /// <param name="certificate"><seecref name="CertificateWithKey"/>.</param>
    /// <returns>Zip archive as a byte array.</returns>
    Task<byte[]> ConvertToPemArchiveAsync(CertificateWithKey certificate);

    /// <summary>
    /// Convert certificate with key to pfx file.
    /// </summary>
    /// <param name="certificate"><seecref name="CertificateWithKey"/>.</param>
    /// <param name="password">Pfx password.</param>
    /// <returns>Pfx file as a byte array.</returns>
    byte[] ConvertToPfx(CertificateWithKey certificate, string password);

    /// <summary>
    /// Convert CRL to pem format.
    /// </summary>
    /// <param name="crl"><see cref="CrlFileModel"/>.</param>
    /// <returns>Pem representation of Crl.</returns>
    byte[] ConvertToPem(CrlFileModel crl);
}
