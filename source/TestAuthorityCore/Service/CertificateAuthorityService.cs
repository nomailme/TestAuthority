using System;
using Org.BouncyCastle.Security;
using Serilog;
using TestAuthority.Application;
using TestAuthority.Application.Random;
using TestAuthority.Domain.Models;
using TestAuthority.Domain.Services;

namespace TestAuthorityCore.Service
{
    /// <summary>
    ///     Service that provides methods for certificate generation.
    /// </summary>
    public class CertificateAuthorityService
    {
        private readonly Func<SecureRandom, CertificateWithKey, CertificateBuilder2> builderFactory;
        private readonly Func<SecureRandom, CertificateWithKey, ICrlBuilder> crlBuilderFactory;
        private readonly RandomService randomService;
        private readonly CertificateWithKey signerCertificate;
        private readonly ILogger logger = Log.ForContext<CertificateAuthorityService>();

        /// <summary>
        ///     Ctor.
        /// </summary>
        public CertificateAuthorityService(CertificateWithKey signerCertificate, RandomService randomService)
        {
            this.signerCertificate = signerCertificate;
            this.randomService = randomService;
            builderFactory = (random, _) => new CertificateBuilder2(random);
            crlBuilderFactory = (random, issuer) => new CrlBuilder(random, issuer);
        }

        /// <summary>
        ///     Generate CRL file.
        /// </summary>
        /// <returns>Crl file as a byte array.</returns>
        public CrlFileModel GenerateCrl()
        {
            var random = randomService.GenerateRandom();
            var crlBuilder = crlBuilderFactory(random, signerCertificate);
            var crl = crlBuilder.Generate();
            return new CrlFileModel(crl);
        }

        /// <summary>
        ///     Generate certificate using certificate request.
        /// </summary>
        /// <param name="requestModel">Certificate Request.</param>
        /// <returns>Certificate <seecref name="CertificateWithKey" />.</returns>
        public CertificateWithKey GenerateSslCertificate(CertificateRequestModel requestModel)
        {
            var notBefore = DateTimeOffset.UtcNow.AddHours(-2);
            var notAfter = DateTimeOffset.UtcNow.AddDays(requestModel.ValidityInDays);
            var random = randomService.GenerateRandom();

            var builder = builderFactory(random, signerCertificate);

            var keyPair = CertificateBuilder2.GenerateKeyPair(2048, random);

            var signerSubject = signerCertificate.Certificate.IssuerDN;

            var certificate = builder.WithSubjectCommonName(requestModel.CommonName)
                .WithKeyPair(keyPair)
                .WithIssuerName(signerSubject)
                .WithNotAfter(notAfter)
                .WithNotBefore(notBefore)
                .WithSubjectAlternativeName(requestModel.Hostnames, requestModel.IpAddresses)
                .WithBasicConstraints(BasicConstrainsConstants.EndEntity)
                .WithExtendedKeyUsage()
                .WithAuthorityKeyIdentifier(signerCertificate.KeyPair)
                .Generate(signerCertificate.KeyPair);

            logger.Information("Issued certificate: \r\n {Certificate}", certificate.Certificate);
            return certificate;


        }
    }
}