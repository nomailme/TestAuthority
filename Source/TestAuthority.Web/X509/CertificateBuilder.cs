using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using NLog;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Operators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Prng;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.X509;
using Org.BouncyCastle.X509.Extension;
using X509Certificate = Org.BouncyCastle.X509.X509Certificate;

namespace TestAuthority.Web.X509
{
    public class CertificateBuilder
    {
        private const string signatureAlgorithm = "SHA256WithRSA";
        private readonly X509V3CertificateGenerator certificateGenerator = new X509V3CertificateGenerator();
        private readonly SecureRandom random = GenerateRandom();

        public CertificateBuilder(int keyStrength = 2048)
        {
            KeyStrength = keyStrength;
            BigInteger serialNumber = BigIntegers.CreateRandomInRange(BigInteger.One, BigInteger.ValueOf(Int64.MaxValue), random);
            certificateGenerator.SetSerialNumber(serialNumber);
        }

        public X509NameWrapper Issuer { get; set; }

        public int KeyStrength { get; }

        public X509NameWrapper Subject { get; set; }

        public CertificateBuilder AddSubjectAltNameExtension(List<string> hostnames, List<string> ipAddresses)
        {
            List<Asn1Encodable> subjectAlternativeNames = hostnames.Select(x => new GeneralName(GeneralName.DnsName, x))
                .Select(x => x as Asn1Encodable)
                .ToList();

            ipAddresses.Select(x => new GeneralName(GeneralName.IPAddress, x))
                .Select(x => x as Asn1Encodable)
                .ToList()
                .ForEach(x => subjectAlternativeNames.Add(x));


            var subjectAlternativeNamesExtension = new DerSequence(subjectAlternativeNames.ToArray());
            certificateGenerator.AddExtension(X509Extensions.SubjectAlternativeName.Id, false, subjectAlternativeNamesExtension);

            return this;
        }

        public byte[] GenerateCertificate(CertificateWithKey signer, string pfxPassword)
        {
            AsymmetricCipherKeyPair keyPair = GenerateKeyPair(KeyStrength, random);

            X509Name issuerDn = new X509CertificateParser().ReadCertificate(signer.Certificate.RawData)
                .IssuerDN;
            certificateGenerator.SetIssuerDN(issuerDn);
            certificateGenerator.SetSubjectDN(Subject.ToX509Name());
            certificateGenerator.SetPublicKey(keyPair.Public);
            ISignatureFactory signatureFactory = new Asn1SignatureFactory(signatureAlgorithm, signer.KeyPair.Private, random);

            X509Certificate certificate = certificateGenerator.Generate(signatureFactory);

            var x509 = new X509Certificate2(certificate.GetEncoded());
            byte[] result = ConvertToPfx(x509, (RsaPrivateCrtKeyParameters)keyPair.Private, pfxPassword);
            return result;
        }

        public byte[] GenerateSelfSignedCertificate(string pfxPassword)
        {
            AsymmetricCipherKeyPair keyPair = GenerateKeyPair(KeyStrength, random);
            certificateGenerator.SetIssuerDN(Subject.ToX509Name());
            certificateGenerator.SetSubjectDN(Subject.ToX509Name());
            certificateGenerator.SetPublicKey(keyPair.Public);

            certificateGenerator.AddExtension(X509Extensions.KeyUsage, true, new KeyUsage(KeyUsage.CrlSign | KeyUsage.KeyCertSign));

            certificateGenerator.AddExtension(X509Extensions.SubjectKeyIdentifier, false, new SubjectKeyIdentifierStructure(keyPair.Public));
            certificateGenerator.AddExtension(X509Extensions.BasicConstraints, true, new BasicConstraints(true));
            certificateGenerator.AddExtension(X509Extensions.AuthorityKeyIdentifier, true, new AuthorityKeyIdentifierStructure(keyPair.Public));

            ISignatureFactory signatureFactory = new Asn1SignatureFactory(signatureAlgorithm, keyPair.Private, random);

            X509Certificate certificate = certificateGenerator.Generate(signatureFactory);

            var x509 = new X509Certificate2(certificate.GetEncoded());
            byte[] result = ConvertToPfx(x509, (RsaPrivateCrtKeyParameters)keyPair.Private, pfxPassword);
            return result;
        }

        public CertificateBuilder SetExtendedKeyUsage(params KeyPurposeID[] keyPurposeIds)
        {
            var extendedKeyUsage = new ExtendedKeyUsage(keyPurposeIds);
            certificateGenerator.AddExtension(X509Extensions.ExtendedKeyUsage, false, extendedKeyUsage);
            return this;
        }

        public CertificateBuilder SetIssuer(X509NameWrapper subjectDn)
        {
            Issuer = subjectDn;
            return this;
        }

        public CertificateBuilder SetNotAfter(DateTime notAfter)
        {
            certificateGenerator.SetNotAfter(notAfter);
            return this;
        }

        public CertificateBuilder SetNotBefore(DateTime notBefore)
        {
            certificateGenerator.SetNotBefore(notBefore);
            return this;
        }

        public CertificateBuilder SetSubject(X509NameWrapper subjectDn)
        {
            Subject = subjectDn;
            return this;
        }

        private static byte[] ConvertToPfx(X509Certificate2 x509, RsaPrivateCrtKeyParameters rsaparams, string pfxPassword)
        {
            var store = new Pkcs12Store();
            SecureRandom random = GenerateRandom();
            X509Certificate cert = DotNetUtilities.FromX509Certificate(x509);
            string friendlyName = cert.SubjectDN.ToString();
            var certificateEntry = new X509CertificateEntry(cert);

            store.SetCertificateEntry(friendlyName, certificateEntry);
            store.SetKeyEntry(friendlyName,
                new AsymmetricKeyEntry(rsaparams),
                new[]
                {
                    certificateEntry
                });

            using (var stream = new MemoryStream())
            {
                store.Save(stream, pfxPassword.ToCharArray(), random);

                return stream.ToArray();
            }
        }

        private static AsymmetricCipherKeyPair GenerateKeyPair(int keyStrength, SecureRandom random)
        {
            var keyGenerationParameters = new KeyGenerationParameters(random, keyStrength);
            var keyPairGenerator = new RsaKeyPairGenerator();
            keyPairGenerator.Init(keyGenerationParameters);
            AsymmetricCipherKeyPair subjectKeyPair = keyPairGenerator.GenerateKeyPair();
            return subjectKeyPair;
        }

        private static SecureRandom GenerateRandom()
        {
            var randomGenerator = new CryptoApiRandomGenerator();
            var random = new SecureRandom(randomGenerator);
            return random;
        }

        private static DerSequence GetSubjectAlternativeNameExtension(string[] hostnames)
        {
            Asn1Encodable[] subjectAlternativeNames = hostnames.Select(x => new GeneralName(GeneralName.DnsName, x))
                .Select(x => x as Asn1Encodable)
                .ToArray();

            var subjectAlternativeNamesExtension = new DerSequence(subjectAlternativeNames);
            return subjectAlternativeNamesExtension;
        }
    }
}
