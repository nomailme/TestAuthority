using System;
using System.Security.Cryptography.X509Certificates;
using Nelibur.Sword.Extensions;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Operators;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.X509;

namespace TestAuthorityCore.X509
{
    /// <summary>
    ///     Certificate builder.
    /// </summary>
    public class CertificateBuilder2 : ICertificateBuilder
    {
        private const string SignatureAlgorithm = "SHA256WithRSA";
        private readonly X509V3CertificateGenerator certificateGenerator = new X509V3CertificateGenerator();
        private readonly SecureRandom random;
        private AsymmetricCipherKeyPair keyPair;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="random">Random value.</param>
        public CertificateBuilder2(SecureRandom random)
        {
            this.random = random;
            var serialNumber =
                BigIntegers.CreateRandomInRange(BigInteger.One, BigInteger.ValueOf(long.MaxValue), random);
            certificateGenerator.SetSerialNumber(serialNumber);
        }

        /// <inheritdoc />
        public X509Name Issuer { get; set; }

        /// <inheritdoc />
        public AsymmetricKeyParameter PublicKeyInfo => keyPair.Public;

        /// <inheritdoc />
        public X509Name Subject { get; set; }

        /// <inheritdoc />
        public ICertificateBuilder AddExtension(string oid, bool isCritical, Asn1Encodable value)
        {
            certificateGenerator.AddExtension(oid, isCritical, value);
            return this;
        }

        /// <inheritdoc />
        public CertificateWithKey Generate()
        {
            return Generate(keyPair);
        }

        /// <inheritdoc />
        public CertificateWithKey Generate(AsymmetricCipherKeyPair signerKeyPair)
        {
            Validate();

            ISignatureFactory signatureFactory =
                new Asn1SignatureFactory(SignatureAlgorithm, signerKeyPair.Private, random);
            certificateGenerator.SetPublicKey(keyPair.Public);

            var certificate = certificateGenerator.Generate(signatureFactory);
            certificate.Verify(signerKeyPair.Public);
            var x509 = new X509Certificate2(certificate.GetEncoded());

            var result = new CertificateWithKey
            {
                Certificate = x509,
                KeyPair = keyPair
            };
            return result;
        }

        /// <inheritdoc />
        public ICertificateBuilder SetIssuer(X509Name issuer)
        {
            Issuer = issuer;
            certificateGenerator.SetIssuerDN(issuer);
            return this;
        }

        /// <inheritdoc />
        public ICertificateBuilder SetNotAfter(DateTimeOffset notAfter)
        {
            certificateGenerator.SetNotAfter(notAfter.UtcDateTime);
            return this;
        }

        /// <inheritdoc />
        public ICertificateBuilder SetNotBefore(DateTimeOffset notBefore)
        {
            certificateGenerator.SetNotBefore(notBefore.UtcDateTime);
            return this;
        }

        /// <inheritdoc />
        public ICertificateBuilder SetSubject(X509Name subject)
        {
            Subject = subject;
            certificateGenerator.SetSubjectDN(subject);
            return this;
        }

        /// <inheritdoc />
        public ICertificateBuilder WithBasicConstraints(BasicConstrainsConstants constrains)
        {
            if (constrains == BasicConstrainsConstants.EndEntity)
            {
                certificateGenerator.AddExtension(X509Extensions.BasicConstraints, true, new BasicConstraints(false));
                return this;
            }

            certificateGenerator.AddExtension(X509Extensions.BasicConstraints, true, new BasicConstraints(1));
            return this;
        }

        /// <inheritdoc />
        public ICertificateBuilder WithKeyPair(AsymmetricCipherKeyPair value)
        {
            keyPair = value;
            return this;
        }

        /// <summary>
        ///     Generate key pair.
        /// </summary>
        /// <param name="keyStrength">Key stregth.</param>
        /// <param name="random"><see cref="SecureRandom" />.</param>
        /// <returns><see cref="AsymmetricCipherKeyPair" />.</returns>
        public static AsymmetricCipherKeyPair GenerateKeyPair(int keyStrength, SecureRandom random)
        {
            var keyGenerationParameters = new KeyGenerationParameters(random, keyStrength);
            var keyPairGenerator = new RsaKeyPairGenerator();
            keyPairGenerator.Init(keyGenerationParameters);
            var subjectKeyPair = keyPairGenerator.GenerateKeyPair();
            return subjectKeyPair;
        }

        private void Validate()
        {
            if (Issuer.IsNull()) throw new InvalidOperationException("Issuer is empty");

            if (Subject.IsNull()) throw new InvalidOperationException("Issuer is empty");

            if (PublicKeyInfo.IsNull()) throw new InvalidOperationException("PublicKeyInfo is empty");
        }
    }
}