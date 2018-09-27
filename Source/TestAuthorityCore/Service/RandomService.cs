using System;
using Org.BouncyCastle.Crypto.Prng;
using Org.BouncyCastle.Security;

namespace TestAuthorityCore.Service
{
    public class RandomService
    {
        private readonly SecureRandom random;
        private readonly CryptoApiRandomGenerator randomGenerator = new CryptoApiRandomGenerator();

        public RandomService()
        {
            random = new SecureRandom(randomGenerator);
        }

        public SecureRandom GenerateRandom()
        {
            return random;
        }
    }
}
