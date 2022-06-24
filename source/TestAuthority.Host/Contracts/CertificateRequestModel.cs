using System;
using System.ComponentModel;
using TestAuthority.Domain.Models;

namespace TestAuthority.Host.Contracts
{
    /// <summary>
    /// CertificateRequest
    /// </summary>
    public class CertificateRequestModel
    {
        /// <summary>
        /// Common Name
        /// </summary>
        public string CommonName { get; set; }

        /// <summary>
        /// Password that will be used for PFX file.
        /// </summary>
        [DefaultValue("123123123")]
        public string Password { get; set; }

        /// <summary>
        /// List of domain names to include in Subject Alternative Name extension.
        /// </summary>
        public string[] Hostname { get; set; } = Array.Empty<string>();

        /// <summary>
        /// List of IP addresses to include in Subject Alternative Name extension.
        /// </summary>
        ///
        public string[] IpAddress { get; set; } = Array.Empty<string>();

        /// <summary>
        /// Output filename (without extension).
        /// </summary>
        [DefaultValue("certificate")]
        public string Filename { get; set; } = "certificate";

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
        [DefaultValue(OutputFormat.Pem)]
        public OutputFormat Format { get; set; } = OutputFormat.Pem;
    }

}