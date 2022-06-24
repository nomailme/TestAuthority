using Org.BouncyCastle.Security;

namespace TestAuthority.Domain.Services;

public interface IRandomService
{
    /// <summary>
    ///     Generate random value.
    /// </summary>
    /// <returns>Random value.</returns>
    SecureRandom GenerateRandom();
}