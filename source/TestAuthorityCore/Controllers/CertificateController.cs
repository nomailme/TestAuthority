using System.Linq;
using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using TestAuthorityCore.Contracts;
using TestAuthorityCore.Service;
using TestAuthorityCore.X509;

namespace TestAuthorityCore.Controllers
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
        public CertificateController(CertificateAuthorityService service, RootCertificateService rootCertificateService,
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
            var result = rootCertificateService.GetRootCertificate().Certificate.RawData;
            return File(result, MediaTypeNames.Application.Octet, "root.cer");
        }

        /// <summary>
        ///     Generate current Crl.
        /// </summary>
        /// <returns>Certificate.</returns>
        [HttpGet("/api/certificate/crl")]
        public IActionResult GetCrl()
        {
            var result = service.GenerateCrl();
            return File(result, MediaTypeNames.Application.Octet, "root.crl");
        }


        /// <summary>
        ///     Issue a certificate. Export in PFX format.
        /// </summary>
        /// <param name="request">Certificate request.</param>
        /// <returns>Result.</returns>
        [HttpGet]
        public IActionResult IssueCertificate([FromQuery] CertificateRequestModel request)
        {
            var certificateRequest = new CertificateRequest
            {
                CommonName = request.CommonName,
                Hostnames = request.Hostname.ToList(),
                IpAddresses = request.IpAddress.ToList(),
                Password = request.Password,
                ValidtyInDays = request.ValidityInDays
            };

            var certificateWithKey = service.GenerateSslCertificate(certificateRequest);
            if (request.Format == CertificateFormat.Pfx)
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