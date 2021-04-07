namespace TestAuthorityCore.X509
{
    /// <summary>
    ///     Represent certificate output format
    /// </summary>
    public enum CertificateFormat
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
}