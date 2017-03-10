using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mime;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Asn1.X509;
using TestAuthority.Web.X509;

namespace TestAuthority.Web.Controllers
{
    [Route("api/certificate")]
    public class EndpointCertificateController : Controller
    {
        private const string DefaultPassword = "123123123";
        private const string SubjectName = "TestAuthority";
        private readonly CertificateWithKey issuer;

        public EndpointCertificateController(RootCertificateManager rootCertificateManager)
        {
            issuer = rootCertificateManager.GetRootCertificate(SubjectName);
        }

        [HttpGet]
        public async Task<FileResult> GetAsync(string password, string[] hostname)
        {
            IPAddress connectionRemoteIpAddress = HttpContext.Connection.RemoteIpAddress;
            IPHostEntry hostEntryAsync = await Dns.GetHostEntryAsync(connectionRemoteIpAddress);
            string dnsHostname = hostEntryAsync.HostName;

            var hostnames = new List<string>
            {
                "localhost",
                connectionRemoteIpAddress.ToString(),
                dnsHostname.Split('.')[0]
            };

            if (hostname.Any())
            {
                hostname.ToList().ForEach(hostnames.Add);
            }

            DateTime now = DateTime.UtcNow.AddDays(-2);

            if (string.IsNullOrWhiteSpace(password))
            {
                password = DefaultPassword;
            }

            byte[] certificate = new CertificateBuilder().SetNotBefore(now)
                .SetNotAfter(now.AddYears(2))
                .SetSubject(new X509NameWrapper().Add(X509Name.CN, dnsHostname))
                .AddSubjectAltNameExtension(hostnames.ToList())
                .GenerateCertificate(issuer, password);

            return File(certificate, MediaTypeNames.Application.Octet, "cert.pfx");
        }

        [HttpGet("root")]
        public FileResult GetRootCertificate()
        {
            byte[] certificate = issuer.Certificate.RawData;

            return File(certificate, MediaTypeNames.Application.Octet, "root.cer");
        }
    }
}
