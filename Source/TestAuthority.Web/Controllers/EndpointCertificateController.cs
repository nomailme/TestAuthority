using System;
using System.Linq;
using System.Net.Mime;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TestAuthority.Web.Actors;
using TestAuthority.Web.Actors.Message;

namespace TestAuthority.Web.Controllers
{
    /// <summary>
    /// Main controller that provides users with certificates.
    /// </summary>
    [Route("api/certificate")]
    public class EndpointCertificateController : Controller
    {
        private readonly ActorManager actorManager;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="actorManager"><see cref="ActorManager"/>.</param>
        public EndpointCertificateController(ActorManager actorManager)
        {
            this.actorManager = actorManager;
        }

        /// <summary>
        /// Send request for endpoint SSL certificate.
        /// </summary>
        /// <param name="password">Password that will be used in pfx file.</param>
        /// <param name="hostname">Hostnames to include in certificate.</param>
        /// <param name="ipAddress">IP addresses to include in certificate.</param>
        /// <returns>PFX certificate.</returns>
        [HttpGet]
        public async Task<FileResult> Get(string password, string[] hostname, string[] ipAddress)
        {
            var request = new IssueSslCertificateRequest
            {
                Hostnames = hostname.ToList(),
                IpAddress = ipAddress.ToList(),
                IncludeLocalhost = true,
                Password = password
            };
            IssueSslCertificateResponse response = await actorManager.GetActor<EndpointCertificateIssueActor>().RequestAsync<IssueSslCertificateResponse>(request);

            return File(response.RawData, MediaTypeNames.Application.Octet, response.Filename);
        }

        /// <summary>
        /// Get root certificate/issue.
        /// </summary>
        /// <returns>Root certificate.</returns>
        [HttpGet("root")]
        public async Task<FileResult> GetRootCertificateAsync()
        {
            GetRootCertificateResponse result = await actorManager.GetActor<RootCertificateProviderActor>()
                .RequestAsync<GetRootCertificateResponse>(new GetRootCertificateRequest());

            return File(result.Certificate.Certificate.RawData, MediaTypeNames.Application.Octet, "root.cer");
        }
    }
}
