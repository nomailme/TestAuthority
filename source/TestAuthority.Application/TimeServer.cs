using TestAuthority.Domain;

namespace TestAuthority.Application;

/// <inheritdoc />
public class TimeServer: ITimeServer
{
    /// <inheritdoc />
    public DateTimeOffset Now => DateTimeOffset.Now;
}
