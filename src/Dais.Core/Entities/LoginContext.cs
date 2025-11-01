namespace DioRed.Dais.Core.Entities;

public class LoginContext
{
    public required UserData User { get; init; }
    public required string? AuthorizeCallback { get; init; }
    public required ApplicationData? Application { get; init; }
    public required DateTimeOffset ExpiresAt { get; init; }

    public record UserData(string UserName, string DisplayName);
    public record ApplicationData(string ApplicationName, string ApplicationCallback);
}