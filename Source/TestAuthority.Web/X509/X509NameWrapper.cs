using System;
using System.Collections.Generic;
using System.Linq;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.X509;

namespace TestAuthority.Web.X509
{
    public class X509NameWrapper
    {
        private readonly Dictionary<DerObjectIdentifier, string> components = new Dictionary<DerObjectIdentifier, string>();

        public X509NameWrapper Add(DerObjectIdentifier componentName, string value)
        {
            components.Add(componentName, value);
            return this;
        }

        public X509Name ToX509Name()
        {
            DerObjectIdentifier[] keys = components.Keys.ToArray();
            string[] values = components.Values.ToArray();

            return new X509Name(keys, values);
        }
    }
}
