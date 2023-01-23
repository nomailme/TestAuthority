using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.X509;
using TestAuthority.Application.Store;
using TestAuthority.Domain.Models;
using TestAuthority.Domain.Services;

namespace TestAuthority.Application.CertificateConverters;

public class PfxConverter : ICertificateConverter
{
    private readonly IRandomService randomService;
    private readonly ISignerProvider signerProvider;
    public PfxConverter(ISignerProvider signerProvider, IRandomService randomService)
    {
        this.signerProvider = signerProvider;
        this.randomService = randomService;
    }

    public async Task<byte[]> Convert(CertificateWithKey input, ICertificateConverterOptions? options = null)
    {
        if (options is not PfxConverterOptions pfxOptions)
        {
            throw new ArgumentException($"Use {nameof(PfxContainerOptions)} for options");
        }

        var password = pfxOptions.Password ?? string.Empty;

        return await ConvertToPfxCoreAsync(input.Certificate, (RsaPrivateCrtKeyParameters)input.KeyPair.Private, password);
    }

    private Task<byte[]> ConvertToPfxCoreAsync(X509Certificate certificate, AsymmetricKeyParameter rsaParams, string pfxPassword)
    {
        var store = new Pkcs12Store();
        var random = randomService.GenerateRandom();
        var friendlyName = certificate.SubjectDN.ToString();
        var signerInfo = signerProvider.GetCertificateSignerInfo();
        var rootCertificate = signerInfo.GetRootCertificate();
        var intermediateCertificates = signerInfo.GetIntermediateCertificates();

        store.SetCertificateEntry(rootCertificate.SubjectDN.ToString(), new X509CertificateEntry(rootCertificate));

        foreach (var intermediateCertificate in intermediateCertificates)
        {
            store.SetCertificateEntry(intermediateCertificate.SubjectDN.ToString(), new X509CertificateEntry(intermediateCertificate));
        }

        var certificateEntry = new X509CertificateEntry(certificate);

        store.SetCertificateEntry(friendlyName, certificateEntry);
        store.SetKeyEntry(friendlyName, new AsymmetricKeyEntry(rsaParams), new[] { certificateEntry });

        using var stream = new MemoryStream();
        store.Save(stream, pfxPassword.ToCharArray(), random);
        return Task.FromResult(stream.ToArray());
    }

    public class PfxConverterOptions : ICertificateConverterOptions
    {
        public PfxConverterOptions(string? password)
        {
            Password = password;
        }
        public string? Password { get; }
    }
}
