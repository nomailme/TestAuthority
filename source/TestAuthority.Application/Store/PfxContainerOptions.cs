using TestAuthority.Domain.Store;

namespace TestAuthority.Application.Store;

/// <summary>
/// Options that are used in <see cref="PfxCertificateStore"/>.
/// </summary>
public class PfxContainerOptions : IContainerOptions
{
    /// <summary>
    /// Pfx password.
    /// </summary>
    public string? PfxPassword { get; init; }
}
