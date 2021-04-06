using System;
using Org.BouncyCastle.Asn1;

namespace TestAuthorityCore.X509
{
    /// <summary>
    /// A wrapper around extension.
    /// </summary>
    public class ExtensionWrapper
    {
        /// <summary>
        /// Whether extension is critical.
        /// </summary>
        public bool IsCritical { get; set; }

        /// <summary>
        /// Oid of extension.
        /// </summary>
        public string Oid { get; set; }

        /// <summary>
        /// Extension value.
        /// </summary>
        public Asn1Encodable Value { get; set; }
    }
}
