namespace Dais.Core.Domain;

public class RegisteredClient
{
    public required string ClientId { get; init; }
    public required bool IsPublic { get; init; }
    public required bool RequirePkce { get; init; }
    public required string? SecretHash { get; init; }
    public required List<string> RedirectUris { get; init; }
    public required HashSet<string> AllowedGrantTypes { get; init; }
    public required HashSet<string> AllowedScopes { get; init; }
}