using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Proto;
using TestAuthority.Web.Actors.Message;
using TestAuthority.Web.X509;

namespace TestAuthority.Web.Actors
{
    /// <summary>
    /// Actor that issues certificates.
    /// </summary>
    public class EndpointCertificateIssueActor : IActor
    {
        private const string DefaultPassword = "123123123";
        private readonly ActorManager actorManager;

        private readonly ILogger<EndpointCertificateIssueActor> logger;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="logger">Logger.</param>
        /// <param name="actorManager"><see cref="ActorManager"/>.</param>
        public EndpointCertificateIssueActor(ILogger<EndpointCertificateIssueActor> logger, ActorManager actorManager)
        {
            this.logger = logger;
            this.actorManager = actorManager;

            logger.LogDebug("Actor created {actorName}", GetType().Name);
        }

        /// <summary>
        /// Receive method.
        /// </summary>
        /// <param name="context">Context.</param>
        /// <returns>Task result.</returns>
        public async Task ReceiveAsync(IContext context)
        {
            switch (context.Message)
            {
                case EndpointCertificateRequest request:
                    byte[] rawData = await IssueCertificate(request);
                    context.Respond(new EndpointCertificateResponse { RawData = rawData, Filename = "cert.pfx" });
                    return;
            }
        }

        private async Task<byte[]> IssueCertificate(EndpointCertificateRequest request)
        {
            var hostnames = new List<string>();
            if (request.IncludeLocalhost)
            {
                hostnames.Add("localhost");
            }
            logger.LogInformation($"Issue certificate request for {request.SubjectCommonName}");
            hostnames.AddRange(request.Hostnames.Select(x => x.ToLowerInvariant()));

            GetRootCertificateResponse certificateWithKey = await actorManager.GetActor<RootCertificateProviderActor>()
                .RequestAsync<GetRootCertificateResponse>(new GetRootCertificateRequest());

            byte[] certificate = new CertificateBuilder()
                .SetNotBefore(request.NotBefore)
                .SetNotAfter(request.NotAfter)
                .SetSubject(new X509NameWrapper().AddCommonName(request.SubjectCommonName))
                .AddSubjectAltNameExtension(hostnames, request.IpAddress)
                .SetExtendedKeyUsage(new ExtendedKeyUsageWrapper().AddClientAuthentication().AddClientAuthentication())
                .GenerateCertificate(certificateWithKey.Certificate, DefaultPassword);

            return certificate;
        }
    }
}
