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

        var chain = BuildChainCertificate(certificate)
            .Select(x => new X509CertificateEntry(x))
            .ToArray();

        store.SetKeyEntry(friendlyName, new AsymmetricKeyEntry(rsaParams), chain);

        using var stream = new MemoryStream();
        store.Save(stream, pfxPassword.ToCharArray(), random);
        return Task.FromResult(stream.ToArray());
    }

    private IEnumerable<X509Certificate> BuildChainCertificate(X509Certificate certificate)
    {
        var signerInfo = signerProvider.GetCertificateSignerInfo();

        var rootCertificate = signerInfo.GetRootCertificate();
        var intermediateCertificates = signerInfo.GetIntermediateCertificates();
        yield return certificate;
        foreach (var intermediateCertificate in intermediateCertificates)
        {
            yield return intermediateCertificate;
        }
        yield return rootCertificate;
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
