using Org.BouncyCastle.X509;

namespace TestAuthorityCore.X509
{
    /// <summary>
    ///     Crl wrapper.
    /// </summary>
    public class CrlFile
    {
        /// <summary>
        ///     ctor.
        /// </summary>
        /// <param name="crl"><see cref="X509Crl" />.</param>
        public CrlFile(X509Crl crl)
        {
            Crl = crl;
        }

        /// <summary>
        ///     Crl file.
        /// </summary>
        public X509Crl Crl { get; set; }
    }
}