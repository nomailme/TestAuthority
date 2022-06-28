using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Operators;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.X509;
using TestAuthority.Domain.Models;
using TestAuthority.Domain.Services;

namespace TestAuthority.Application;

/// <summary>
///     Certificate builder.
/// </summary>
public class CertificateBuilder2 : ICertificateBuilder
{
    private const string SignatureAlgorithm = "SHA256WithRSA";
    private readonly X509V3CertificateGenerator certificateGenerator = new();
    private readonly SecureRandom random;

    private X509Name? issuerName;
    private AsymmetricCipherKeyPair? keyPair;

    private X509Name? subjectName;

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

    /// <summary>
    ///     Get public key.
    /// </summary>
    /// <returns></returns>
    public AsymmetricKeyParameter? GetPublicKeyInfo()
    {
        return keyPair?.Public;
    }

    /// <inheritdoc />
    public ICertificateBuilder AddExtension(string oid, Asn1Encodable value, bool isCritical = false)
    {
        certificateGenerator.AddExtension(oid, isCritical, value);
        return this;
    }

    /// <inheritdoc />
    public CertificateWithKey Generate(AsymmetricCipherKeyPair signerKeyPair)
    {
        Validate();

        ISignatureFactory signatureFactory = new Asn1SignatureFactory(SignatureAlgorithm, signerKeyPair.Private, random);
        certificateGenerator.SetPublicKey(keyPair!.Public);

        var certificate = certificateGenerator.Generate(signatureFactory);
        certificate.Verify(signerKeyPair.Public);

        var result = new CertificateWithKey(certificate, keyPair);
        return result;
    }

    /// <inheritdoc />
    public ICertificateBuilder WithIssuerName(X509Name issuer)
    {
        ArgumentNullException.ThrowIfNull(issuer);
        issuerName = issuer;
        certificateGenerator.SetIssuerDN(issuer);
        return this;
    }

    /// <inheritdoc />
    public ICertificateBuilder WithNotAfter(DateTimeOffset notAfter)
    {
        certificateGenerator.SetNotAfter(notAfter.UtcDateTime);
        return this;
    }

    /// <inheritdoc />
    public ICertificateBuilder WithNotBefore(DateTimeOffset notBefore)
    {
        certificateGenerator.SetNotBefore(notBefore.UtcDateTime);
        return this;
    }

    /// <inheritdoc />
    public ICertificateBuilder WithSubject(X509Name subject)
    {
        ArgumentNullException.ThrowIfNull(subject);
        subjectName = subject;
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
    /// <param name="keyStrength">Key strength.</param>
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

    /// TODO: refactor
    private void Validate()
    {
        if (issuerName == null) throw new InvalidOperationException("Issuer is empty");

        if (subjectName == null) throw new InvalidOperationException("Issuer is empty");

        if (GetPublicKeyInfo() == null) throw new InvalidOperationException("PublicKeyInfo is empty");

        if (keyPair == null) throw new InvalidOperationException("KeyPair must not be null");
    }
}
