using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Prng;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Security;
using TestAuthorityCore.Extensions;
using TestAuthorityCore.X509;
using X509Certificate = Org.BouncyCastle.X509.X509Certificate;

namespace TestAuthorityCore.Service
{
    /// <summary>
    /// Provides methods for root certificate management. 
    /// </summary>
    public class RootCertificateService
    {
        private const string Password = "123123123";
        private const string RootCertificateName = "Root.pfx";
        private readonly Func<SecureRandom, ICertificateBuilder> builderFactory;

        /// <summary>
        /// Ctor.
        /// </summary>
        public RootCertificateService()
        {
            builderFactory = (random) => new CertificateBuilder2(random);
        }

        /// <summary>
        /// Get root certificate.
        /// </summary>
        /// <returns>Root certificate.</returns>
        public CertificateWithKey GetRootCertificate()
        {
            string rootCertificatePath = GetRootCertificatePath();
            if (File.Exists(rootCertificatePath))
            {
                byte[] rawData = File.ReadAllBytes(rootCertificatePath);
                CertificateWithKey result = Convert(rawData);
                return result;
            }

            byte[] newRawData = GenerateRootCertificate();
            Directory.CreateDirectory(Path.GetDirectoryName(rootCertificatePath));
            File.WriteAllBytes(rootCertificatePath, newRawData);
            return Convert(newRawData);
        }

        private static CertificateWithKey Convert(byte[] pfxCertificate)
        {
            using var stream = new MemoryStream(pfxCertificate);
            var store = new Pkcs12Store();
            store.Load(stream, Password.ToCharArray());
            string alias = store.Aliases.OfType<string>().Single();
            X509CertificateEntry certificateEntry = store.GetCertificate(alias);
            AsymmetricKeyEntry keyEntry = store.GetKey(alias);

            var result = new CertificateWithKey
            {
                Certificate = new X509Certificate2(certificateEntry.Certificate.GetEncoded()),
                KeyPair = new AsymmetricCipherKeyPair(certificateEntry.Certificate.GetPublicKey(), keyEntry.Key)
            };
            return result;
        }

        private static byte[] ConvertToPfx(X509Certificate2 x509, RsaPrivateCrtKeyParameters rsaParams, string pfxPassword)
        {
            var store = new Pkcs12Store();
            SecureRandom random = GenerateRandom();
            X509Certificate cert = DotNetUtilities.FromX509Certificate(x509);
            string friendlyName = cert.SubjectDN.ToString();
            var certificateEntry = new X509CertificateEntry(cert);

            store.SetCertificateEntry(friendlyName, certificateEntry);
            store.SetKeyEntry(friendlyName,
                new AsymmetricKeyEntry(rsaParams),
                new[]
                {
                    certificateEntry
                });

            using var stream = new MemoryStream();
            store.Save(stream, pfxPassword.ToCharArray(), random);

            return stream.ToArray();
        }

        private static SecureRandom GenerateRandom()
        {
            var randomGenerator = new CryptoApiRandomGenerator();
            var random = new SecureRandom(randomGenerator);
            return random;
        }

        private static string GetRootCertificatePath()
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "test-authority", RootCertificateName);
        }

        private byte[] GenerateRootCertificate()
        {
            string commonName = $"Test Authority {DateTime.UtcNow:MM/yyyy}";
            DateTimeOffset notBefore = DateTimeOffset.UtcNow.AddHours(-2);
            DateTimeOffset notAfter = DateTimeOffset.UtcNow.AddYears(5);
            SecureRandom random = GenerateRandom();

            ICertificateBuilder builder = builderFactory(random);

            AsymmetricCipherKeyPair keyPair = CertificateBuilder2.GenerateKeyPair(2048, GenerateRandom());

            CertificateWithKey certificate = builder
                .WithSubjectCommonName(commonName)
                .WithKeyPair(keyPair)
                .SetNotAfter(notAfter)
                .SetNotBefore(notBefore)
                .WithBasicConstraints(BasicConstrainsConstants.CertificateAuthority)
                //.WithKeyUsage()
                .WithAuthorityKeyIdentifier(keyPair)
                .WithSubjectKeyIdentifier()
                .SetIssuer(builder.Subject)
                .Generate();

            return ConvertToPfx(certificate.Certificate, (RsaPrivateCrtKeyParameters)keyPair.Private, Password);
        }
    }
}
