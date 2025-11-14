namespace Dais.Core.Domain.Interfaces;

public interface ITokenService
{
    Task<AuthorizationCode?> FindAuthorizationCodeAsync(
        string? code,
        CancellationToken ct = default
    );

    Task<TokenResponse> IssueTokensAsync(
        RegisteredClient client,
        AuthorizationCode code,
        string? codeVerifier,
        string? scope,
        CancellationToken ct = default
    );
}