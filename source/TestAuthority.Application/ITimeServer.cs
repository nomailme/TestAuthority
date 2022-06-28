namespace TestAuthority.Application;

public interface ITimeServer
{
    DateTimeOffset Now { get; }
}
