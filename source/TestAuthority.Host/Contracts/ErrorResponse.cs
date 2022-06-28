using System.Collections.Generic;

namespace TestAuthority.Host.Contracts;

/// <summary>
/// Error response contract.
/// </summary>
public class ErrorResponse
{
    /// <summary>
    /// List of errors.
    /// </summary>
    // ReSharper disable once CollectionNeverQueried.Global
    public List<ErrorModel> Errors { get; } = new();
}
