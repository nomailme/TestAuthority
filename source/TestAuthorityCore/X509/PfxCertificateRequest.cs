using System.Collections.Generic;

namespace TestAuthorityCore.X509
{
    /// <summary>
    /// Request for certificate.
    /// </summary>
    public class CertificateRequest
    {
        /// <summary>
        /// Common name of the certificate.
        /// </summary>
        public string CommonName { get; set; }

        /// <summary>
        /// List of domain names to be included in Subject Alternative Name extension
        /// </summary>
        public List<string> Hostnames { get; set; }

        /// <summary>
        /// List of IP addresses to be included in Subject Alternative Name extension
        /// </summary>
        public List<string> IpAddresses { get; set; }

        /// <summary>
        /// Password for pfx file.
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Certificate validtiy in days. 
        /// </summary>
        /// <value>Default value of 364.</value>
        public int ValidtyInDays { get; set; } = 364;
    }
}
