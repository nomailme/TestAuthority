using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.X509;
using TestAuthority.Domain.Models;

namespace TestAuthority.Application.Extensions;

public static class CertificateSignerInfoExtensions
{
    public static X509Certificate GetRootCertificate(this CertificateSignerInfo signerInfo)
    {
        return signerInfo.CertificateChain.Select(x => x.Certificate).Last();
    }

    public static IEnumerable<CertificateWithKey> GetIntermediateCertificates(this CertificateSignerInfo signerInfo)
    {
        if (signerInfo.CertificateChain.Count == 1)
        {
            return new List<CertificateWithKey>();
        }

        return signerInfo.CertificateChain.Take(Range.EndAt(signerInfo.CertificateChain.Count - 1));
    }

    public static X509Certificate GetSignerCertificate(this CertificateSignerInfo signerInfo)
    {
        return signerInfo.CertificateChain.First().Certificate;
    }

    public static AsymmetricCipherKeyPair GetSignerKeyPair(this CertificateSignerInfo signerInfo)
    {
        return signerInfo.CertificateChain.First().KeyPair;
    }

    public static AsymmetricKeyParameter GetSignerPrivateKey(this CertificateSignerInfo signerInfo)
    {
        return signerInfo.GetSignerKeyPair().Private;
    }
}
