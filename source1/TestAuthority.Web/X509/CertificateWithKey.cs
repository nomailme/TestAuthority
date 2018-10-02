using System;
using System.Security.Cryptography.X509Certificates;
using Org.BouncyCastle.Crypto;

namespace TestAuthority.Web.X509
{
    public class CertificateWithKey
    {
        public X509Certificate2 Certificate { get; set; }
        public AsymmetricCipherKeyPair KeyPair { get; set; }
    }
}
