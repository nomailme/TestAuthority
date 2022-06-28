using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.X509;

namespace TestAuthority.Domain.Models;

public record CertificateSignerInfo
{
    public CertificateWithKey? CertificateWithKey { get; set; }

    public X509Name? Subject => CertificateWithKey?.Certificate.SubjectDN;

    public List<X509Certificate> Chain { get; set; } = new();
}
