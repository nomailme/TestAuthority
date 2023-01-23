using MediatR;
using TestAuthority.Domain.CertificateConverters;
using TestAuthority.Domain.Models;
using TestAuthority.Domain.Services;

namespace TestAuthority.Application.CertificateConverters;

/// <summary>
///     Service that provides conversion of certificates to <seecref name="CerficateWithKey" />.
/// </summary>
public class CertificateConverterService : ICertificateConverterService
{
    private readonly PemConverter pemConverter;
    private readonly PfxConverter pfxConverter;

    /// <summary>
    ///     Ctor.
    /// </summary>
    /// <param name="randomService"><seecref name="RandomService" />.</param>
    /// <param name="signerProvider"><see cref="ISignerProvider" />.</param>
    /// <param name="mediator"><see cref="IMediator" />.</param>
    public CertificateConverterService(IRandomService randomService, ISignerProvider signerProvider, IMediator mediator)
    {
        pemConverter = new PemConverter(signerProvider, mediator);
        pfxConverter = new PfxConverter(signerProvider, randomService);
    }

    /// <summary>
    ///     Convert certificate to zip archive with certificate and key in PEM format.
    /// </summary>
    /// <param name="certificate"><seecref name="CerficateWithKey" />.</param>
    /// <returns></returns>
    public async Task<byte[]> ConvertToPemArchiveAsync(CertificateWithKey certificate)
    {
        return await pemConverter.Convert(certificate);
    }

    /// <summary>
    ///     Convert certificate to pfx file.
    /// </summary>
    /// <param name="certificate">
    ///     <seecref name="certificate" />
    /// </param>
    /// <param name="password">Password for pfx file.</param>
    /// <returns>Pfx file as a byte array.</returns>
    public async Task<byte[]> ConvertToPfx(CertificateWithKey certificate, string password)
    {
        return await pfxConverter.Convert(certificate, new PfxConverter.PfxConverterOptions(password));
    }

    /// <inheritdoc />
    public Task<byte[]> ConvertToPem(CrlFileModel crl)
    {
        return pemConverter.Convert(crl);

    }
}
