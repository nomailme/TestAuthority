using Org.BouncyCastle.Crypto.Prng;
using Org.BouncyCastle.Security;
using TestAuthority.Domain.Services;

namespace TestAuthority.Application.Random;

/// <summary>
///     Service that provides random values.
/// </summary>
public class RandomService : IRandomService
{
    private readonly SecureRandom random;
    private readonly CryptoApiRandomGenerator randomGenerator = new();

    /// <summary>
    ///     Ctor.
    /// </summary>
    public RandomService()
    {
        random = new SecureRandom(randomGenerator);
    }


    /// <inheritdoc />
    public SecureRandom GenerateRandom()
    {
        return random;
    }
}