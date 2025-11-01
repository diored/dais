namespace DioRed.Dais.Core.Entities;

public class AuthCode
{
    public required string ClientId { get; init; }
    public required string CodeChallenge { get; init; }
    public required string CodeChallengeMethod { get; init; }
    public required string RedirectUri { get; init; }
    public required DateTime Expiry { get; init; }
    public required string UserName { get; init; }
    public required string DisplayName { get; init; }
    public required string ApplicationName { get; init; }
}