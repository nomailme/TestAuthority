using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Operators;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.X509;
using Org.BouncyCastle.X509.Extension;
using TestAuthority.Domain.Models;
using TestAuthority.Domain.Services;

namespace TestAuthority.Application;

/// <summary>
/// Class that can create CRL files.
/// </summary>
public class CrlBuilder : ICrlBuilder
{
    private const string SignatureAlgorithm = "SHA256WithRSA";
    private readonly X509V2CrlGenerator crlGenerator = new();
    private readonly SecureRandom random;
    private readonly CertificateWithKey signerCertificate;

    /// <summary>
    /// Ctor.
    /// </summary>
    /// <param name="random">Random value.</param>
    /// <param name="signerCertificate">Signers certificate.</param>
    public CrlBuilder(SecureRandom random, CertificateWithKey signerCertificate)
    {
        this.random = random;
        this.signerCertificate = signerCertificate;
        crlGenerator.SetIssuerDN(signerCertificate.Certificate.IssuerDN);
    }

    /// <summary>
    /// Generate CRL.
    /// </summary>
    /// <returns>Result.</returns>
    public X509Crl Generate()
    {

        ISignatureFactory signatureFactory =
            new Asn1SignatureFactory(SignatureAlgorithm, signerCertificate.KeyPair.Private, random);

        crlGenerator.SetThisUpdate(DateTime.Now);
        crlGenerator.SetNextUpdate(DateTime.Now.AddYears(1));

        crlGenerator.AddCrlEntry(BigInteger.One, DateTime.Now, CrlReason.PrivilegeWithdrawn);

        crlGenerator.AddExtension(X509Extensions.AuthorityKeyIdentifier,
            false,
            new AuthorityKeyIdentifierStructure(signerCertificate.Certificate));

        var crlNumber = new BigInteger(DateTime.UtcNow.ToString("yyyyMMddHHmm"));
        crlGenerator.AddExtension(X509Extensions.CrlNumber, false, new CrlNumber(crlNumber));

        X509Crl crlTemp = crlGenerator.Generate(signatureFactory);
        return crlTemp;
    }
}