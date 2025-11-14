namespace Dais.Core.Domain.Interfaces;

public interface IAuthorizationService
{
    Task<AuthorizationCode> CreateAuthorizationCodeAsync(
        RegisteredClient client,
        string redirectUri,
        string codeChallenge,
        string subjectId,
        string? scope,
        CancellationToken ct = default
    );
}