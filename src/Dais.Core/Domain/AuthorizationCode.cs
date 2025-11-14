namespace Dais.Core.Domain;

public class AuthorizationCode
{
    public required string Value { get; init; }
    public required string ClientId { get; init; }
    public required string RedirectUri { get; init; }
    public required DateTimeOffset ExpiresAt { get; init; }
    public required string CodeChallenge { get; init; }
    public required string CodeChallengeMethod { get; init; } // "S256"
    public bool Consumed { get; set; }
    public string? SubjectId { get; init; }
    public string? Scope { get; init; }
}