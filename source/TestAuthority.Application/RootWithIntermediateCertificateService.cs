using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.X509;
using TestAuthority.Application.Store;
using TestAuthority.Domain.Models;
using TestAuthority.Domain.Services;
using TestAuthority.Domain.Store;

namespace TestAuthority.Application;

/// <summary>
///     Provides methods for root certificate management.
/// </summary>
public class RootWithIntermediateCertificateService : ISignerProvider
{
    private const string RootCertificateName = "Root";
    private const string IntermediateCertificateName = "intermediate";
    private const string Password = "123123123";

    private readonly Func<SecureRandom, ICertificateBuilder> builderFactory;
    private readonly ICertificateStore certificateStore;
    private readonly IRandomService randomService;

    /// <summary>
    ///     Ctor.
    /// </summary>
    public RootWithIntermediateCertificateService(ICertificateStore certificateStore, IRandomService randomService)
    {
        this.certificateStore = certificateStore;
        this.randomService = randomService;
        builderFactory = random => new CertificateBuilder2(random);
    }

    /// <summary>
    ///     Get root certificate.
    /// </summary>
    /// <returns>Root certificate.</returns>
    public CertificateSignerInfo GetRootCertificate()
    {
        if (!certificateStore.TryGet(RootCertificateName, new PfxContainerOptions{ PfxPassword = Password}, out var rootCertificate))
        {
            rootCertificate = GenerateRootCertificate();
            certificateStore.SaveCertificate(RootCertificateName, rootCertificate, new PfxContainerOptions{ PfxPassword = Password});
        }

        if (!certificateStore.TryGet(IntermediateCertificateName, new PfxContainerOptions{ PfxPassword = Password}, out var intermediateCertificate))
        {
            intermediateCertificate = GenerateIntermediateCertificate(rootCertificate);
            certificateStore.SaveCertificate(IntermediateCertificateName, intermediateCertificate, new PfxContainerOptions{ PfxPassword = Password});
        }

        return new CertificateSignerInfo { CertificateWithKey = intermediateCertificate, Chain = new List<X509Certificate> { rootCertificate.Certificate } };
    }

    private CertificateWithKey GenerateIntermediateCertificate(CertificateWithKey signerCertificate)
    {
        var random = randomService.GenerateRandom();
        var commonName = $"Intermediate Test Authority {DateTime.UtcNow:MM/yyyy}";
        var notBefore = DateTimeOffset.UtcNow.AddHours(-2);
        var notAfter = DateTimeOffset.UtcNow.AddYears(5);

        var builder = builderFactory(random);

        var keyPair = CertificateBuilder2.GenerateKeyPair(2048, random);

        var certificate = builder
            .WithSubjectCommonName(commonName)
            .WithKeyPair(keyPair)
            .WithNotAfter(notAfter)
            .WithNotBefore(notBefore)
            .WithBasicConstraints(BasicConstrainsConstants.CertificateAuthority)
            .WithAuthorityKeyIdentifier(signerCertificate.KeyPair)
            .WithSubjectKeyIdentifier()
            .WithIssuerName(signerCertificate.Certificate.SubjectDN)
            .Generate(signerCertificate.KeyPair);

        return certificate;
    }

    private CertificateWithKey GenerateRootCertificate()
    {
        var random = randomService.GenerateRandom();
        var commonName = $"Test Authority {DateTime.UtcNow:MM/yyyy}";
        var notBefore = DateTimeOffset.UtcNow.AddHours(-2);
        var notAfter = DateTimeOffset.UtcNow.AddYears(5);

        var builder = builderFactory(random);

        var keyPair = CertificateBuilder2.GenerateKeyPair(2048, random);

        var issuerName = BuildCommonName(commonName);

        var certificate = builder
            .WithSubject(issuerName)
            .WithKeyPair(keyPair)
            .WithNotAfter(notAfter)
            .WithNotBefore(notBefore)
            .WithBasicConstraints(BasicConstrainsConstants.CertificateAuthority)
            .WithAuthorityKeyIdentifier(keyPair)
            .WithSubjectKeyIdentifier()
            .WithIssuerName(issuerName)
            .Generate(keyPair);

        return certificate;
    }

    private static X509Name BuildCommonName(string commonName)
    {
        ArgumentNullException.ThrowIfNull(commonName);
        var issuerName = new Dictionary<DerObjectIdentifier, string>
        {
            { X509Name.CN, commonName }
        };
        return CertificateBuilder2Extensions.GetX509Name(issuerName);
    }
}
