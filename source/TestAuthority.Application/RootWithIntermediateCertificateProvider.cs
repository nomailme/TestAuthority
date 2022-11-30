using Microsoft.Extensions.Options;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.Security;
using TestAuthority.Application.Store;
using TestAuthority.Domain.Models;
using TestAuthority.Domain.Services;
using TestAuthority.Domain.Store;

namespace TestAuthority.Application;

/// <summary>
///     Provides methods for root certificate management.
/// </summary>
public class RootWithIntermediateCertificateProvider : ISignerProvider
{
    private const string RootCertificateName = "Root";
    private const string IntermediateCertificateName = "intermediate";
    private const string Password = "123123123";

    private readonly Func<SecureRandom, ICertificateBuilder> builderFactory;
    private readonly ICertificateStore certificateStore;
    private readonly IRandomService randomService;
    private readonly IOptions<CrlSettings> crlSettings;

    /// <summary>
    ///     Ctor.
    /// </summary>
    public RootWithIntermediateCertificateProvider(ICertificateStore certificateStore, IRandomService randomService, IOptions<CrlSettings> crlSettings)
    {
        this.certificateStore = certificateStore;
        this.randomService = randomService;
        this.crlSettings = crlSettings;
        builderFactory = random => new CertificateBuilder2(random);
    }

    /// <summary>
    ///     Get root certificate.
    /// </summary>
    /// <returns>Root certificate.</returns>
    public CertificateSignerInfo GetCertificateSignerInfo()
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

        var chain = new List<CertificateWithKey> { intermediateCertificate, rootCertificate };


        return new CertificateSignerInfo(chain);
    }

    private CertificateWithKey GenerateIntermediateCertificate(CertificateWithKey signerCertificate)
    {
        var random = randomService.GenerateRandom();
        var commonName = $"Intermediate Test Authority {DateTime.UtcNow:MM/yyyy}";
        var notBefore = DateTimeOffset.UtcNow.AddHours(-2);
        var notAfter = DateTimeOffset.UtcNow.AddYears(10);

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
            .WithCrlDistributionPoint(GetCrlAddresses(crlSettings.Value, signerCertificate.Certificate.SerialNumber))
            .Generate(signerCertificate.KeyPair);

        return certificate;
    }

    private static List<string> GetCrlAddresses(CrlSettings settings, BigInteger certificateSerialNumber)
    {
        return new List<string> { $"{settings.CaAddress}/api/crl/{certificateSerialNumber.ToString(16)}" };
    }

    private CertificateWithKey GenerateRootCertificate()
    {
        var random = randomService.GenerateRandom();
        var commonName = $"Test Authority {DateTime.UtcNow:MM/yyyy}";
        var notBefore = DateTimeOffset.UtcNow.AddHours(-2);
        var notAfter = DateTimeOffset.UtcNow.AddYears(15);

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
