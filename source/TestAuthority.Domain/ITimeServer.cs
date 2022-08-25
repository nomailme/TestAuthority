namespace TestAuthority.Domain;

public interface ITimeServer
{
    DateTimeOffset Now { get; }
}
