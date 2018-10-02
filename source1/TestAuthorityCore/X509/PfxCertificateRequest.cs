using System;
using System.Collections.Generic;

namespace TestAuthorityCore.X509
{
    public class PfxCertificateRequest
    {
        public string CommonName { get; set; }

        public List<string> Hostnames { get; set; }

        public List<string> IpAddresses { get; set; }
        public string Password { get; set; }
    }
}
