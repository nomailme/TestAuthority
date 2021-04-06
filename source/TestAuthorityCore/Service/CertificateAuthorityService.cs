using System;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.X509;
using TestAuthorityCore.Extensions;
using TestAuthorityCore.X509;

namespace TestAuthorityCore.Service
{
    /// <summary>
    /// Service that provides methods for certificate generation.
    /// </summary>
    public class CertificateAuthorityService
    {
        private readonly Func<SecureRandom, CertificateWithKey, CertificateBuilder2> builderFactory;
        private readonly Func<SecureRandom, CertificateWithKey, ICrlBuilder> crlBuilderFactory;
        private readonly RandomService randomService;
        private readonly CertificateWithKey SignerCertificate;

        /// <summary>
        /// Ctor.
        /// </summary>
        public CertificateAuthorityService(CertificateWithKey signerCertificate, RandomService randomService)
        {
            SignerCertificate = signerCertificate;
            this.randomService = randomService;
            builderFactory = (random, issuer) => new CertificateBuilder2(random);
            crlBuilderFactory = (random, issuer) => new CrlBuilder(random, issuer);
        }

        /// <summary>
        /// Generate CRL file.
        /// </summary>
        /// <returns>Crl file as a byte array.</returns>
        public byte[] GenerateCrl()
        {
            SecureRandom random = randomService.GenerateRandom();
            ICrlBuilder crlBuilder = crlBuilderFactory(random, SignerCertificate);
            var crl = crlBuilder.Generate();
            return crl.GetEncoded();
        }

        /// <summary>
        /// Generate certificate using certificate request.
        /// </summary>
        /// <param name="request">Certificate Request.</param>
        /// <returns>Certificate <seecref name="CertificateWithKey"/>.</returns>
        public CertificateWithKey GenerateSslCertificate(PfxCertificateRequest request)
        {
            DateTimeOffset notBefore = DateTimeOffset.UtcNow.AddHours(-2);
            DateTimeOffset notAfter = DateTimeOffset.UtcNow.AddDays(request.ValidtyInDays);
            SecureRandom random = randomService.GenerateRandom();

            CertificateBuilder2 builder = builderFactory(random, SignerCertificate);

            AsymmetricCipherKeyPair keyPair = CertificateBuilder2.GenerateKeyPair(2048, random);

            X509Name signerSubject = new X509CertificateParser().ReadCertificate(SignerCertificate.Certificate.RawData)
                .IssuerDN;

            CertificateWithKey certificate = builder.WithSubjectCommonName(request.CommonName)
                .WithKeyPair(keyPair)
                .SetIssuer(signerSubject)
                .SetNotAfter(notAfter)
                .SetNotBefore(notBefore)
                .WithSubjectAlternativeName(request.Hostnames, request.IpAddresses)
                .WithBasicConstraints(BasicConstrainsConstants.EndEntity)
                .WithExtendedKeyUsage()
                .WithAuthorityKeyIdentifier(SignerCertificate.KeyPair)
                .Generate(SignerCertificate.KeyPair);
            return certificate;
        }
    }
}
