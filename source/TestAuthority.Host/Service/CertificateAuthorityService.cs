using System;
using Org.BouncyCastle.Security;
using TestAuthority.Application;
using TestAuthority.Domain.Models;
using TestAuthority.Domain.Services;

namespace TestAuthority.Host.Service;

/// <summary>
///     Service that provides methods for certificate generation.
/// </summary>
public class CertificateAuthorityService
{
    private readonly Func<SecureRandom, CertificateWithKey, ICrlBuilder> crlBuilderFactory;
    private readonly IRandomService randomService;
    private readonly CertificateWithKey signerCertificate;

    /// <summary>
    ///     Ctor.
    /// </summary>
    public CertificateAuthorityService(CertificateWithKey signerCertificate, IRandomService randomService)
    {
        this.signerCertificate = signerCertificate;
        this.randomService = randomService;
        crlBuilderFactory = (random, issuer) => new CrlBuilder(random, issuer);
    }

    /// <summary>
    ///     Generate CRL file.
    /// </summary>
    /// <returns>Crl file as a byte array.</returns>
    public CrlFileModel GenerateCrl()
    {
        var random = randomService.GenerateRandom();
        var crlBuilder = crlBuilderFactory(random, signerCertificate);
        var crl = crlBuilder.Generate();
        return new CrlFileModel(crl);
    }
}
