using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Mime;
using Microsoft.AspNetCore.Mvc;
using Nelibur.Sword.Extensions;
using TestAuthorityCore.Service;
using TestAuthorityCore.X509;

namespace TestAuthorityCore.Controllers
{
    /// <summary>
    /// Provides functionality to work with certificates
    /// </summary>
    [Route("api/certificate")]
    public class CertificateController : Controller
    {
        private readonly RootCertificateService rootCertificateService;
        private readonly ICertificateConverter converter;
        private readonly CertificateAuthorityService service;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="service"><seecref name="CertificateAuthorityService"/>.</param>
        /// <param name="rootCertificateService"><seecref name="RootCertificateService"/>.</param>
        /// <param name="converter"><seecref name="ICertificateConverter"/>.</param>
        public CertificateController(CertificateAuthorityService service, RootCertificateService rootCertificateService, ICertificateConverter converter)
        {
            this.service = service;
            this.rootCertificateService = rootCertificateService;
            this.converter = converter;
        }

        /// <summary>
        /// Download root certificate.
        /// </summary>
        /// <returns>Root certificate.</returns>
        [HttpGet("/api/certificate/root")]
        public IActionResult GetRootCertificate()
        {
            byte[] result = rootCertificateService.GetRootCertificate().Certificate.RawData;
            return File(result, MediaTypeNames.Application.Octet, "root.cer");
        }

        /// <summary>
        /// Generate current Crl.
        /// </summary>
        /// <returns>Certificate.</returns>
        [HttpGet("/api/certificate/crl")]
        public IActionResult GetCrl()
        {
            byte[] result = service.GenerateCrl();
            return File(result, MediaTypeNames.Application.Octet, "root.crl");
        }


        /// <summary>
        /// Issue a certificate. Export in PFX format.
        /// </summary>
        /// <param name="request">Certificate request.</param>
        /// <returns>Result.</returns>
        [HttpGet]
        public IActionResult IssueCertificate([FromQuery] CertificateRequest request)
        {
            if (request.Hostname.IsNullOrEmpty())
            {
                return BadRequest("At least one hostname is required");
            }

            if (request.CommonName.IsNullOrEmpty())
            {
                request.CommonName = $"SSL Certificate ({request.Hostname.First()})";
            }

            if (request.Password.IsNullOrEmpty())
            {
                request.Password = "123123123";
            }

            if (request.IpAddress.IsNullOrEmpty())
            {
                request.IpAddress = new string[0];
            }

            var request1 = new PfxCertificateRequest
            {
                CommonName = request.CommonName,
                Hostnames = request.Hostname.ToList(),
                IpAddresses = request.IpAddress.ToList(),
                Password = request.Password,
                ValidtyInDays = request.ValidityInDays
            };

            var certificateWithKey = service.GenerateSslCertificate(request1);
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

        /// <summary>
        /// CertificateRequest
        /// </summary>
        public class CertificateRequest
        {
            /// <summary>
            /// Common Name
            /// </summary>
            [Required]
            public string CommonName { get; set; }

            /// <summary>
            /// Password that will be used for PFX file.
            /// </summary>
            [DefaultValue("123123213")]
            public string Password { get; set; }

            /// <summary>
            /// List of domain names to include in Subject Alternative Name extension.
            /// </summary>
            public string[] Hostname { get; set; } = new string[0];

            /// <summary>
            /// List of IP addresses to include in Subject Alternative Name extension.
            /// </summary>
            public string[] IpAddress { get; set; } = new string[0];

            /// <summary>
            /// Output filename (without extension).
            /// </summary>
            [DefaultValue("certificate")]
            public string Filename { get; set; }

            /// <summary>
            /// Certificate validity in days.
            /// </summary>
            [DefaultValue(365)]
            public int ValidityInDays { get; set; }

            /// <summary>
            /// Output format.
            /// </summary>
            /// <remarks>
            /// Pfx will produce PFX file
            /// Pem will produce ZIP file with certificate,key and root certificate.
            /// </remarks>
            [DefaultValue(CertificateFormat.Pfx)]
            public CertificateFormat Format { get; set; } = CertificateFormat.Pfx;
        }

    }
}
