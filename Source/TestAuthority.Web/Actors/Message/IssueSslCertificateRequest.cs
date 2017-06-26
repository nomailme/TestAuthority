using System;
using System.Collections.Generic;

namespace TestAuthority.Web.Actors.Message
{
    /// <summary>
    /// Request.
    /// </summary>
    public class IssueSslCertificateRequest
    {
        private string password = "123123123";

        /// <summary>
        /// List of hostnames that will end up in Subject Alternative Name extension.
        /// </summary>
        public List<string> Hostnames { get; set; } = new List<string>();

        /// <summary>
        /// Should include localhost hostname in Subject Alternative Name extension.
        /// </summary>
        public bool IncludeLocalhost { get; set; } = true;

        /// <summary>
        /// List of IP addresses that will end up in Subject Alternative Name extension.
        /// </summary>
        public List<string> IpAddress { get; set; } = new List<string>();

        /// <summary>
        /// Certificate validity time.
        /// </summary>
        public DateTime NotAfter { get; set; } = DateTime.UtcNow.AddYears(1);

        /// <summary>
        /// Certificate validity time.
        /// </summary>
        public DateTime NotBefore { get; set; } = DateTime.UtcNow.Subtract(TimeSpan.FromHours(2));

        /// <summary>
        /// Password that will be used for .pfx file.
        /// </summary>
        public string Password
        {
            get => password;
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    return;
                }
                password = value;
            }
        }

        /// <summary>
        /// Subject.
        /// </summary>
        public string SubjectCommonName { get; set; } = $"SSL Certificate ${DateTime.UtcNow}";
    }
}
