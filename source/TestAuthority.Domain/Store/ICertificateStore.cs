using System.Diagnostics.CodeAnalysis;
using TestAuthority.Domain.Models;

namespace TestAuthority.Domain.Store;

/// <summary>
///     Certificate store.
/// </summary>
public interface ICertificateStore
{
    /// <summary>
    ///     Save certificate to the store.
    /// </summary>
    /// <param name="certificateName">Certificate name.</param>
    /// <param name="certificateWithKey">Certificate.</param>
    /// <param name="options">Options.</param>
    void SaveCertificate(ReadOnlySpan<char> certificateName, CertificateWithKey certificateWithKey, IContainerOptions options);

    /// <summary>
    ///     Returns certificate via out parameter.
    /// </summary>
    /// <param name="certificateName">Certificate name.</param>
    /// <param name="options"><see cref="IContainerOptions" />.</param>
    /// <param name="certificateWithKey">Certificate.</param>
    /// <returns>True if certificate was found. False otherwise</returns>
    bool TryGet(string certificateName, IContainerOptions options, [MaybeNullWhen(false)] out CertificateWithKey certificateWithKey);
}
