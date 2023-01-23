using System;
using System.Net.Mime;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using TestAuthority.Application.CrlBuilders;
using TestAuthority.Domain.CertificateConverters;
using TestAuthority.Domain.Services;

namespace TestAuthority.Host.Controllers;

/// <summary>
///     Provides functionality to work with Crl.
/// </summary>
[Route("api/crl")]
public class CrlController: Controller
{
    private readonly IMediator mediator;
    private readonly ISignerProvider signerProvider;
    private readonly ICertificateConverterService converterService;

    /// <summary>
    /// Ctor.
    /// </summary>
    /// <param name="mediator"><see cref="IMediator"/>.</param>
    /// <param name="signerProvider"><see cref="ISignerProvider"/>.</param>
    /// <param name="converterService"><see cref="ICertificateConverterService"/>.</param>
    public CrlController(IMediator mediator, ISignerProvider signerProvider, ICertificateConverterService converterService)
    {
        this.mediator = mediator;
        this.signerProvider = signerProvider;
        this.converterService = converterService;
    }

    /// <summary>
    /// Issue a Certificate Revocation List in PEM.
    /// </summary>
    [HttpGet]
    public async Task<FileContentResult> Get()
    {
        var signer = signerProvider.GetCertificateSignerInfo();
        var crl = await mediator.Send(new CrlBuilderRequest(signer));

        var result = await converterService.ConvertToPem(crl);
        return File(result, MediaTypeNames.Application.Octet, "root.crl");
    }

    /// <summary>
    /// Issue a Certificate Revocation List in DER.
    /// </summary>
    [HttpGet("{serialNumber}")]
    public async Task<FileResult> GetCrl(string serialNumber)
    {
        var signer = signerProvider.GetCertificateSignerInfo();
        var crlModel = await mediator.Send(new CrlBuilderRequest(signer, serialNumber));

        return File(crlModel.Crl.GetEncoded(),"application/pkix-crl" , $"{serialNumber}-{DateTimeOffset.UnixEpoch}.crl");
    }
}
