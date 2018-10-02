using System;
using System.Collections.Generic;
using Org.BouncyCastle.Asn1.X509;

namespace TestAuthority.Web.X509
{
    /// <summary>
    /// Wrapper for extended key usage extension.
    /// </summary>
    public class ExtendedKeyUsageWrapper
    {
        private readonly List<KeyPurposeID> ids = new List<KeyPurposeID>();

        public ExtendedKeyUsageWrapper AddClientAuthentication()
        {
            ids.Add(KeyPurposeID.IdKPClientAuth);
            return this;
        }

        public ExtendedKeyUsageWrapper AddServerAuthentication()
        {
            ids.Add(KeyPurposeID.IdKPServerAuth);
            return this;
        }

        public KeyPurposeID[] GetKeyUsages()
        {
            return ids.ToArray();
        }
    }
}
