using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using TestAuthority.Application.CertificateBuilders;
using TestAuthority.Domain.Models;
using TestAuthority.Domain.Services;
using TestAuthority.Host.Service;

namespace TestAuthority.Host.Controllers
{
    /// <summary>
    ///     Provides functionality to work with certificates
    /// </summary>
    [Route("api/certificate")]
    public class CertificateController : Controller
    {
        private readonly ICertificateConverter converter;
        private readonly IMediator mediator;
        private readonly ISignerProvider signerProvider;
        private readonly CertificateAuthorityService service;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="service"><seecref name="CertificateAuthorityService" />.</param>
        /// <param name="signerProvider"><see cref="ISignerProvider"/>.</param>
        /// <param name="converter"><seecref name="ICertificateConverter" />.</param>
        /// <param name="mediator"><see cref="IMediator"/>.</param>
        public CertificateController(CertificateAuthorityService service,
            ISignerProvider signerProvider,
            ICertificateConverter converter,
            IMediator mediator)
        {
            this.service = service;
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
            var result = signerProvider.GetRootCertificate().CertificateWithKey!.Certificate.GetEncoded();
            return File(result, MediaTypeNames.Application.Octet, "root.cer");
        }

        /// <summary>
        ///     Generate current Crl.
        /// </summary>
        /// <returns>Certificate.</returns>
        [HttpGet("/api/certificate/crl")]
        public IActionResult GetCrl()
        {
            var crl = service.GenerateCrl();
            var result = converter.ConvertToPem(crl);
            return File(result, MediaTypeNames.Application.Octet, "root.crl");
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


            var crtRequest = new CertificateBuilderRequest(certificateRequest, signerProvider.GetRootCertificate());
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
                var pem = converter.ConvertToPemArchive(result, resultFilename);
                return File(pem, MediaTypeNames.Application.Zip, resultFilename);
            }
        }
    }
}
