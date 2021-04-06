using System;
using System.Security.Cryptography.X509Certificates;
using Org.BouncyCastle.Crypto;

namespace TestAuthorityCore.X509
{
    /// <summary>
    /// Wrapper for certificate with keys.
    /// </summary>
    public class CertificateWithKey
    {
        /// <summary>
        /// Certificate.
        /// </summary>
        public X509Certificate2 Certificate { get; set; }

        /// <summary>
        /// Key pair.
        /// </summary>
        public AsymmetricCipherKeyPair KeyPair { get; set; }
    }
}
