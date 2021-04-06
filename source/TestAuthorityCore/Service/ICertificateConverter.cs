using TestAuthorityCore.X509;

/// <summary>
/// Provides methods to convert certificates and keys.
/// </summary>
public interface ICertificateConverter
{
    /// <summary>
    /// Convert certificate with key to ZIP archive containing certificate and key in PEM format.
    /// </summary>
    /// <param name="certificate"><seecref name="CertificateWithKey"/>.</param>
    /// <param name="filename">Output filename.</param>
    /// <returns>Zip archive as a byte array.</returns>
    byte[] ConvertToPemArchive(CertificateWithKey certificate, string filename);

    /// <summary>
    /// Convert certificate with key to pfx file.
    /// </summary>
    /// <param name="certificate"><seecref name="CertificateWithKey"/>.</param>
    /// <param name="password">Pfx password.</param>
    /// <returns>Pfx file as a byte array.</returns>
    byte[] ConvertToPfx(CertificateWithKey certificate, string password);
}
