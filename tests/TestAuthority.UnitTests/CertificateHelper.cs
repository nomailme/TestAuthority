using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Operators;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.Utilities;
using Org.BouncyCastle.X509;
using TestAuthority.Domain.Models;

namespace TestAuthority.UnitTests;

public static class CertificateHelper
{
    public static CertificateWithKey GenerateDummyCertificate(string commonName)
    {
        X509V3CertificateGenerator generator = new();
        var keyPair = GenerateKeyPair(2048, new SecureRandom());
        Asn1SignatureFactory signatureFactory = new("SHA256WithRSA",keyPair.Private);

        var dn = GenerateName(commonName);
        generator.SetIssuerDN(dn);
        generator.SetSubjectDN(dn);
        generator.SetSerialNumber(BigIntegers.CreateRandomBigInteger(128,new SecureRandom()));
        generator.SetNotBefore(DateTime.Now.Subtract(TimeSpan.FromDays(1)));
        generator.SetNotAfter(DateTime.Now.Add(TimeSpan.FromDays(1)));
        generator.SetPublicKey(keyPair.Public);
        var certificate = generator.Generate(signatureFactory);
        return new CertificateWithKey(certificate, keyPair);

    }

    private static X509Name GenerateName(string commonName) => new($"CN={commonName}");


    private static AsymmetricCipherKeyPair GenerateKeyPair(int keyStrength, SecureRandom random)
    {
        var keyGenerationParameters = new KeyGenerationParameters(random, keyStrength);
        var keyPairGenerator = new RsaKeyPairGenerator();
        keyPairGenerator.Init(keyGenerationParameters);
        var subjectKeyPair = keyPairGenerator.GenerateKeyPair();
        return subjectKeyPair;
    }
}
