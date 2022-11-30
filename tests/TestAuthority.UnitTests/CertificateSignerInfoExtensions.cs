using TestAuthority.Domain.Models;

namespace TestAuthority.UnitTests;

public class SignerInfoExtensionsTests
{
    [Fact]
    public void GetRootCertificate_CertificateChainWithThreeCertificates_RootCertificate()
    {
        var chain = new List<CertificateWithKey>
        {
            CertificateHelper.GenerateDummyCertificate("Intermediate1"),
            CertificateHelper.GenerateDummyCertificate("Intermediate2"),
            CertificateHelper.GenerateDummyCertificate("Root")
        };
        CertificateSignerInfo signerInfo = new CertificateSignerInfo(chain);

        var rootCertificate = signerInfo.GetRootCertificate();
        Assert.Equal("CN=Root",rootCertificate.SubjectDN.ToString(), StringComparer.OrdinalIgnoreCase);
    }

    [Fact]
    public void GetIntermediateCertificates_CertificateChainWithThreeCertificates_IntermediateCertificates()
    {
        var chain = new List<CertificateWithKey>
        {
            CertificateHelper.GenerateDummyCertificate("Intermediate1"),
            CertificateHelper.GenerateDummyCertificate("Intermediate2"),
            CertificateHelper.GenerateDummyCertificate("Root")
        };
        CertificateSignerInfo signerInfo = new CertificateSignerInfo(chain);

        var intermediateCertificates = signerInfo.GetIntermediateCertificates();
        Assert.DoesNotContain(intermediateCertificates, x=>x.SubjectDN.ToString().Contains("CN=Root"));
        Assert.Equal(2,intermediateCertificates.Count);
        
    }
}
