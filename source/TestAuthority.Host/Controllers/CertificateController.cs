using System.Linq;
using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using TestAuthority.Application;
using TestAuthority.Domain.Models;
using TestAuthority.Host.Service;
using CertificateRequestModel = TestAuthority.Host.Contracts.CertificateRequestModel;

namespace TestAuthority.Host.Controllers
{
    /// <summary>
    ///     Provides functionality to work with certificates
    /// </summary>
    [Route("api/certificate")]
    public class CertificateController : Controller
    {
        private readonly ICertificateConverter converter;
        private readonly RootCertificateService rootCertificateService;
        private readonly CertificateAuthorityService service;

        /// <summary>
        ///     Ctor.
        /// </summary>
        /// <param name="service"><seecref name="CertificateAuthorityService" />.</param>
        /// <param name="rootCertificateService"><seecref name="RootCertificateService" />.</param>
        /// <param name="converter"><seecref name="ICertificateConverter" />.</param>
        public CertificateController(CertificateAuthorityService service,
            RootCertificateService rootCertificateService,
            ICertificateConverter converter)
        {
            this.service = service;
            this.rootCertificateService = rootCertificateService;
            this.converter = converter;
        }

        /// <summary>
        ///     Download root certificate.
        /// </summary>
        /// <returns>Root certificate.</returns>
        [HttpGet("/api/certificate/root")]
        public IActionResult GetRootCertificate()
        {
            var result = rootCertificateService.GetRootCertificate().Certificate.GetEncoded();
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
        public IActionResult IssueCertificate(Contracts.CertificateRequestModel request)
        {
            var certificateRequest = new TestAuthority.Domain.Models.CertificateRequestModel
            {
                CommonName = request.CommonName,
                Hostnames = request.Hostname.ToList(),
                IpAddresses = request.IpAddress.ToList(),
                ValidityInDays = request.ValidityInDays
            };


            var certificateWithKey = service.GenerateSslCertificate(certificateRequest);


            if (request.Format == OutputFormat.Pfx)
            {
                var resultFilename = string.Concat(request.Filename.Trim('.'), ".pfx");
                var pfx = converter.ConvertToPfx(certificateWithKey, request.Password);
                return File(pfx, MediaTypeNames.Application.Octet, resultFilename);
            }
            else
            {
                var resultFilename = string.Concat(request.Filename.Trim('.'), ".zip");
                var pem = converter.ConvertToPemArchive(certificateWithKey, resultFilename);
                return File(pem, MediaTypeNames.Application.Zip, resultFilename);
            }
        }
    }
}