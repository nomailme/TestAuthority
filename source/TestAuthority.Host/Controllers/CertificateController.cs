using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using TestAuthority.Application.CertificateBuilders;
using TestAuthority.Application.Extensions;
using TestAuthority.Domain.CertificateConverters;
using TestAuthority.Domain.Models;
using TestAuthority.Domain.Services;

namespace TestAuthority.Host.Controllers;

/// <summary>
///     Provides functionality to work with certificates
/// </summary>
[Route("api/certificate")]
public class CertificateController : Controller
{
    private readonly ICertificateConverter converter;
    private readonly IMediator mediator;
    private readonly ISignerProvider signerProvider;

    /// <summary>
    ///     Ctor.
    /// </summary>
    /// <param name="signerProvider"><see cref="ISignerProvider"/>.</param>
    /// <param name="converter"><seecref name="ICertificateConverter" />.</param>
    /// <param name="mediator"><see cref="IMediator"/>.</param>
    public CertificateController(
        ISignerProvider signerProvider,
        ICertificateConverter converter,
        IMediator mediator)
    {
        this.signerProvider = signerProvider;
        this.converter = converter;
        this.mediator = mediator;
    }

    /// <summary>
    ///     Download root certificate.
    /// </summary>
    /// <returns>Root certificate.</returns>
    [HttpGet("/api/certificate/root")]
    public IActionResult GetRootCertificate()
    {
        var result = signerProvider.GetCertificateSignerInfo().GetSignerCertificate().GetEncoded();
        return File(result, MediaTypeNames.Application.Octet, "root.cer");
    }

    /// <summary>
    ///     Issue a certificate. Export in PFX format.
    /// </summary>
    /// <param name="request">Certificate request.</param>
    /// <returns>Result.</returns>
    [HttpGet]
    public async Task<IActionResult> IssueCertificate(Contracts.CertificateRequestModel request)
    {
        var certificateRequest = new CertificateRequestModel
        {
            CommonName = request.CommonName,
            Hostnames = request.Hostname.ToList(),
            IpAddresses = request.IpAddress.ToList(),
            ValidityInDays = request.ValidityInDays
        };


        var crtRequest = new CertificateBuilderRequest(certificateRequest, signerProvider.GetCertificateSignerInfo().GetSignerCertificate());
        var result = await mediator.Send(crtRequest);


        if (request.Format == OutputFormat.Pfx)
        {
            var resultFilename = string.Concat(request.Filename.Trim('.'), ".pfx");
            var pfx = converter.ConvertToPfx(result, request.Password);
            return File(pfx, MediaTypeNames.Application.Octet, resultFilename);
        }
        else
        {
            var resultFilename = string.Concat(request.Filename.Trim('.'), ".zip");
            var pem = await converter.ConvertToPemArchiveAsync(result);
            return File(pem, MediaTypeNames.Application.Zip, resultFilename);
        }
    }
}
