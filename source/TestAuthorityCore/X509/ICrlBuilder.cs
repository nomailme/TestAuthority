using Org.BouncyCastle.X509;

namespace TestAuthorityCore.X509
{
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
}