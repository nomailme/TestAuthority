using System;
using Org.BouncyCastle.Asn1;

namespace TestAuthorityCore.X509
{
    public class ExtensionWrapper
    {
        public bool IsCritical { get; set; }

        public string Oid { get; set; }

        public Asn1Encodable Value { get; set; }
    }
}
