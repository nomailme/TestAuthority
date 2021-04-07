using Org.BouncyCastle.Crypto.Prng;
using Org.BouncyCastle.Security;

namespace TestAuthorityCore.Service
{
    /// <summary>
    ///     Service that provides random values.
    /// </summary>
    public class RandomService
    {
        private readonly SecureRandom random;
        private readonly CryptoApiRandomGenerator randomGenerator = new CryptoApiRandomGenerator();

        /// <summary>
        ///     Ctor.
        /// </summary>
        public RandomService()
        {
            random = new SecureRandom(randomGenerator);
        }

        /// <summary>
        ///     Generate random value.
        /// </summary>
        /// <returns>Random value.</returns>
        public SecureRandom GenerateRandom()
        {
            return random;
        }
    }
}