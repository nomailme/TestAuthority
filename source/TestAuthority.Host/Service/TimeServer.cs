using System;
using TestAuthority.Application;

namespace TestAuthority.Host.Service;

/// <inheritdoc />
public class TimeServer: ITimeServer
{
    /// <inheritdoc />
    public DateTimeOffset Now { get; } = DateTimeOffset.Now;
}
