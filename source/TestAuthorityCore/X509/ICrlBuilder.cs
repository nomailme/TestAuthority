using System;
using Org.BouncyCastle.X509;

namespace TestAuthorityCore.X509
{
    public interface ICrlBuilder
    {
        X509Crl Generate();
    }
}
