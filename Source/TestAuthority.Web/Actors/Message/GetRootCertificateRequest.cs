using System;
using TestAuthority.Web.X509;

namespace TestAuthority.Web.Actors.Message
{
    /// <summary>
    /// Request for root certificte.
    /// </summary>
    public sealed class GetRootCertificateRequest
    {
    }

    /// <summary>
    /// Respose with root certificate.
    /// </summary>
    public sealed class GetRootCertificateResponse
    {
        /// <summary>
        /// Certificate data.
        /// </summary>
        public CertificateWithKey Certificate { get; set; }
    }
}
