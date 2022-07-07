using System.Net.Mime;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using TestAuthority.Application.CrlBuilders;
using TestAuthority.Domain.Services;
using TestAuthority.Host.Service;

namespace TestAuthority.Host.Controllers;

/// <summary>
///     Provides functionality to work with Crl.
/// </summary>
[Route("api/crl")]
public class CrlController: Controller
{
    private readonly IMediator mediator;
    private readonly ISignerProvider signerProvider;
    private readonly ICertificateConverter converter;

    /// <summary>
    /// Ctor.
    /// </summary>
    /// <param name="mediator"><see cref="IMediator"/>.</param>
    /// <param name="signerProvider"><see cref="ISignerProvider"/>.</param>
    /// <param name="converter"><see cref="ICertificateConverter"/>.</param>
    public CrlController(IMediator mediator, ISignerProvider signerProvider, ICertificateConverter converter)
    {
        this.mediator = mediator;
        this.signerProvider = signerProvider;
        this.converter = converter;
    }

    /// <summary>
    /// Issue a Certificate Revocation List.
    /// </summary>
    [HttpGet]
    public async Task<FileContentResult> Get()
    {
        var signer = signerProvider.GetCertificateSignerInfo();
        var crl = await mediator.Send(new CrlBuilderRequest(signer));

        var result = converter.ConvertToPem(crl);
        return File(result, MediaTypeNames.Application.Octet, "root.crl");
    }
}
