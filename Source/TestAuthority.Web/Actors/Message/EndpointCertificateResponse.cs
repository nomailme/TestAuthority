using System;
using System.Collections.Generic;
using System.Linq;

namespace TestAuthority.Web.Actors.Message
{
    /// <summary>
    /// Response.
    /// </summary>
    public class EndpointCertificateResponse
    {

        /// <summary>
        /// Certificate raw data.
        /// </summary>
        public byte[] RawData { get; set; }

        /// <summary>
        /// Certificate filename.
        /// </summary>
        public string Filename { get; set; }
    }
}