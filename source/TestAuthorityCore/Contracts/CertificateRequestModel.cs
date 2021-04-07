using System.ComponentModel;
using System.Linq;

namespace TestAuthorityCore.Contracts
{
    /// <summary>
    /// CertificateRequest
    /// </summary>
    public class CertificateRequestModel
    {
        private string commonName = string.Empty;
        /// <summary>
        /// Common Name
        /// </summary>
        public string CommonName
        {
            get
            {
                if (string.IsNullOrEmpty(commonName))
                {
                    var allSanRecords = Hostname.ToList().Concat(IpAddress);
                    var firstRecord = allSanRecords.First();
                    return $"{firstRecord} certificate";
                }
                return commonName;
            }
            set
            {
                commonName = value;
            }
        }

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
