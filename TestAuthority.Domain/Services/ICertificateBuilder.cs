using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using TestAuthority.Domain.Models;

namespace TestAuthority.Domain.Services;

/// <summary>
///     Builds certificates since 1968.
/// </summary>
public interface ICertificateBuilder
{
    /// <summary>
    ///     Add extension.
    /// </summary>
    /// <param name="oid">Oid.</param>
    /// <param name="isCritical">Whether this extension should be marked as critical.</param>
    /// <param name="value">Extension value.</param>
    /// <returns><seecref name="ICertificateBuilder" />.</returns>
    ICertificateBuilder AddExtension(string oid,  Asn1Encodable value, bool isCritical = false);

    /// <summary>
    ///     Generate certificate.
    /// </summary>
    /// <param name="signerPrivateKey">Signer.</param>
    /// <returns><seecref name="CertificateWithKey" />.</returns>
    CertificateWithKey Generate(AsymmetricCipherKeyPair signerPrivateKey);

    /// <summary>
    ///     Set issuer.
    /// </summary>
    /// <param name="issuer">Issuer.</param>
    /// <returns><seecref name="ICertificateBuilder" />.</returns>
    ICertificateBuilder WithIssuerName(X509Name issuer);

    /// <summary>
    ///     Set cerficate not after date.
    /// </summary>
    /// <param name="notAfter">Date.</param>
    /// <returns><seecref name="ICertificateBuilder" />.</returns>
    ICertificateBuilder WithNotAfter(DateTimeOffset notAfter);


    /// <summary>
    ///     Set cerficate not before date.
    /// </summary>
    /// <param name="notBefore">Date.</param>
    /// <returns><seecref name="ICertificateBuilder" />.</returns>
    ICertificateBuilder WithNotBefore(DateTimeOffset notBefore);

    /// <summary>
    ///     Set subject.
    /// </summary>
    /// <param name="subject">Subject.</param>
    /// <returns><seecref name="ICertificateBuilder" />.</returns>
    ICertificateBuilder WithSubject(X509Name subject);

    /// <summary>
    ///     Set basic contstraints for the certificate.
    /// </summary>
    /// <param name="constrains"><seecref name="BasicContstrainsConstants" />.</param>
    /// <returns><seecref name="ICertificateBuilder" />.</returns>
    ICertificateBuilder WithBasicConstraints(BasicConstrainsConstants constrains);

    /// <summary>
    ///     Provide keypair for the certificate.
    /// </summary>
    /// <param name="keyPair">Keypair</param>
    /// <returns><seecref name="ICertificateBuilder" />.</returns>
    ICertificateBuilder WithKeyPair(AsymmetricCipherKeyPair keyPair);

    /// <summary>
    /// Get public key.
    /// </summary>
    /// <returns></returns>
    AsymmetricKeyParameter? GetPublicKeyInfo();

    /// <summary>
    /// Get the distinguished name of the issuer.
    /// </summary>
    /// <returns></returns>
    X509Name? GetIssuerName();
}

/// <summary>
///     Enum for basic constrains.
/// </summary>
public enum BasicConstrainsConstants
{
    /// <summary>
    ///     End entity.
    /// </summary>
    EndEntity = 1,

    /// <summary>
    ///     Certificate authority.
    /// </summary>
    CertificateAuthority
}