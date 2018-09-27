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
        X509Name Issuer { get; set; }
        AsymmetricKeyParameter PublicKeyInfo { get; }
        X509Name Subject { get; set; }
        ICertificateBuilder AddExtension(string oid, bool isCritical, Asn1Encodable value);
        CertificateWithKey Generate();
        CertificateWithKey Generate(AsymmetricCipherKeyPair signerPrivateKey);
        ICertificateBuilder SetIssuer(X509Name issuer);
        ICertificateBuilder SetNotAfter(DateTimeOffset notAfter);
        ICertificateBuilder SetNotBefore(DateTimeOffset notBefore);
        ICertificateBuilder SetSubject(X509Name subject);
        ICertificateBuilder WithBasicConstraints(BasicConstrainsConstants constrains);
        ICertificateBuilder WithKeyPair(AsymmetricCipherKeyPair keyPair);
    }

    public enum BasicConstrainsConstants
    {
        EndEntity = 1,
        CertificateAuthority
    }
}
