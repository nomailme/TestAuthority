using System;
using System.Threading.Tasks;
using Proto;
using TestAuthority.Web.Actors.Message;
using TestAuthority.Web.X509;

namespace TestAuthority.Web.Actors
{
    /// <summary>
    /// Actor that helps with root certicate.
    /// </summary>
    public class RootCertificateProviderActor : IActor
    {
        private const string SubjectName = "TestAuthority";
        private readonly RootCertificateManager rootCertificateManager;

        /// <summary>
        /// Ctor.
        /// </summary>
        /// <param name="rootCertificateManager">Root certificate manager.</param>
        public RootCertificateProviderActor(RootCertificateManager rootCertificateManager)
        {
            this.rootCertificateManager = rootCertificateManager;
        }

        /// <summary>
        /// Receives message.
        /// </summary>
        /// <param name="context"><see cref="IContext"/>.</param>
        /// <returns>Task result.</returns>
        public Task ReceiveAsync(IContext context)
        {
            switch (context.Message)
            {
                case GetRootCertificateRequest _:
                    CertificateWithKey result = rootCertificateManager.GetRootCertificate(SubjectName);
                    var message = new GetRootCertificateResponse
                    {
                        Certificate = result
                    };
                    context.Respond(message);
                    return Actor.Done;
            }
            return Actor.Done;
        }
    }
}
