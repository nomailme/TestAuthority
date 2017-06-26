using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Asn1.X509;
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
        
        private readonly ILogger<EndpointCertificateIssueActor> logger;
        private readonly ActorManager actorManager;

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
                case IssueSslCertificateRequest request:


                    byte[] rawData = await IssueCertificate(request);
                    context.Respond(new IssueSslCertificateResponse { RawData = rawData, Filename = "cert.pfx" });
                    return;
            }
        }

        private async Task<byte[]> IssueCertificate(IssueSslCertificateRequest request)
        {
            var hostnames = new List<string>();
            if (request.IncludeLocalhost)
            {
                hostnames.Add("localhost");
            }
            logger.LogInformation($"Issue certificate request for {request.SubjectCommonName}");
            hostnames.AddRange(request.Hostnames.Select(x => x.ToLowerInvariant()));

            var certificateWithKey = await actorManager.GetActor<RootCertificateProviderActor>()
                .RequestAsync<GetRootCertificateResponse>(new GetRootCertificateRequest());

            byte[] certificate = new CertificateBuilder()
                .SetNotBefore(request.NotBefore)
                .SetNotAfter(request.NotAfter)
                .SetSubject(new X509NameWrapper().Add(X509Name.CN, request.SubjectCommonName))
                .AddSubjectAltNameExtension(hostnames, request.IpAddress)
                .SetExtendedKeyUsage(KeyPurposeID.IdKPServerAuth, KeyPurposeID.IdKPClientAuth)
                .GenerateCertificate(certificateWithKey.Certificate, DefaultPassword);

            return certificate;
        }
    }
}
