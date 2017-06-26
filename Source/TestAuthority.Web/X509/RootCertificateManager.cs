using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Pkcs;

namespace TestAuthority.Web.X509
{
    public sealed class RootCertificateManager
    {
        private const string RootCertificateName = "Root.pfx";
        private static readonly string Password = "123123123";

        public CertificateWithKey GetRootCertificate(string subjectName)
        {
            string rootCertificatePath = GetRootCertificatePath();
            if (File.Exists(rootCertificatePath))
            {
                byte[] rawData = File.ReadAllBytes(rootCertificatePath);
                CertificateWithKey result = Convert(rawData);
                return result;
            }
            byte[] newRawData = IssueSelfSignedCertificate(subjectName);
            Directory.CreateDirectory(Path.GetDirectoryName(rootCertificatePath));
            File.WriteAllBytes(rootCertificatePath, newRawData);
            return Convert(newRawData);
        }

        private static CertificateWithKey Convert(byte[] pfxCertificate)
        {
            using (var stream = new MemoryStream(pfxCertificate))
            {
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
        }

        private static string GetRootCertificatePath()
        {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "TestAuthority", RootCertificateName);
        }

        private byte[] IssueSelfSignedCertificate(string subjectName)
        {
            X509NameWrapper subject = new X509NameWrapper()
                .Add(X509Name.CN, subjectName);

            DateTime now = DateTime.UtcNow.AddDays(-2);
            return new CertificateBuilder()
                .SetSubject(subject)
                .SetNotBefore(now)
                .SetNotAfter(now.AddYears(2))
                .SetExtendedKeyUsage(new ExtendedKeyUsageWrapper().AddClientAuthentication().AddServerAuthentication())
                .GenerateSelfSignedCertificate(Password);
        }
    }
}
