using System;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;

namespace TestAuthorityCore.X509
{
    /// <summary>
    /// Builds certificates since 1968.
    /// </summary>
    public interface ICertificateBuilder
    {
        /// <summary>
        /// Cerficate issuer.
        /// </summary>
        X509Name Issuer { get; set; }

        /// <summary>
        /// Public key information.
        /// </summary>

        AsymmetricKeyParameter PublicKeyInfo { get; }
        /// <summary>
        /// Subject of the certificate.
        /// </summary>

        X509Name Subject { get; set; }
        /// <summary>
        /// Add extension.
        /// </summary>
        /// <param name="oid">Oid.</param>
        /// <param name="isCritical">Whether this extension should be marked as critical.</param>
        /// <param name="value">Extension value.</param>
        /// <returns><seecref name="ICertificateBuilder"/>.</returns>

        ICertificateBuilder AddExtension(string oid, bool isCritical, Asn1Encodable value);
        /// <summary>
        /// Generate certificate.
        /// </summary>
        /// <returns><seecref name="CertificateWithKey"/>.</returns>

        /// <summary>
        /// Generate certificate.
        /// </summary>
        /// <returns><seecref name="CertificateWithKey"/>.</returns>
        CertificateWithKey Generate();

        /// <summary>
        /// Generate certificate.
        /// </summary>
        /// <param name="signerPrivateKey">Signer.</param>
        /// <returns><seecref name="CertificateWithKey"/>.</returns>
        CertificateWithKey Generate(AsymmetricCipherKeyPair signerPrivateKey);

        /// <summary>
        /// Set issuer.
        /// </summary>
        /// <param name="issuer">Issuer.</param>
        /// <returns><seecref name="ICertificateBuilder"/>.</returns>
        ICertificateBuilder SetIssuer(X509Name issuer);

        /// <summary>
        /// Set cerficate not after date.
        /// </summary>
        /// <param name="notAfter">Date.</param>
        /// <returns><seecref name="ICertificateBuilder"/>.</returns>
        ICertificateBuilder SetNotAfter(DateTimeOffset notAfter);


        /// <summary>
        /// Set cerficate not before date.
        /// </summary>
        /// <param name="notBefore">Date.</param>
        /// <returns><seecref name="ICertificateBuilder"/>.</returns>
        ICertificateBuilder SetNotBefore(DateTimeOffset notBefore);

        /// <summary>
        /// Set subject.
        /// </summary>
        /// <param name="subject">Subject.</param>
        /// <returns><seecref name="ICertificateBuilder"/>.</returns>
        ICertificateBuilder SetSubject(X509Name subject);

        /// <summary>
        /// Set basic contstraints for the certificate.
        /// </summary>
        /// <param name="constrains"><seecref name="BasicContstrainsConstants"/>.</param>
        /// <returns><seecref name="ICertificateBuilder"/>.</returns>
        ICertificateBuilder WithBasicConstraints(BasicConstrainsConstants constrains);

        /// <summary>
        /// Provide keypair for the certificate.
        /// </summary>
        /// <param name="keyPair">Keypair</param>
        /// <returns><seecref name="ICertificateBuilder"/>.</returns>
        ICertificateBuilder WithKeyPair(AsymmetricCipherKeyPair keyPair);
    }

    /// <summary>
    /// Enum for basic constrains.
    /// </summary>
    public enum BasicConstrainsConstants
    {
        /// <summary>
        /// End entity.
        /// </summary>
        EndEntity = 1,

        /// <summary>
        /// Certificate authority.
        /// </summary>
        CertificateAuthority
    }
}
