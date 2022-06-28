using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.X509;

namespace TestAuthority.Domain.Models;

public record CertificateSignerInfo(CertificateWithKey SignerCertificate, List<X509Certificate>? Chain)
{
    public CertificateWithKey SignerCertificate { get; } = SignerCertificate;

    public X509Name? Subject => SignerCertificate.Certificate.SubjectDN;

    public List<X509Certificate> Chain { get; } = Chain ?? new List<X509Certificate>();
}
