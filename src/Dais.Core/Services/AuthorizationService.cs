using Dais.Core.Domain;
using Dais.Core.Domain.Interfaces;

namespace Dais.Core.Services;

public class AuthorizationService(
    IAuthorizationCodeStore codes
) : IAuthorizationService
{
    public async Task<AuthorizationCode> CreateAuthorizationCodeAsync(
        RegisteredClient client,
        string redirectUri,
        string codeChallenge,
        string subjectId,
        string? scope,
        CancellationToken ct = default
    )
    {
        var code = new AuthorizationCode
        {
            Value = Guid.NewGuid().ToString("n"),
            ClientId = client.ClientId,
            RedirectUri = redirectUri,
            ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(5),
            CodeChallenge = codeChallenge,
            CodeChallengeMethod = "S256",
            SubjectId = subjectId,
            Scope = scope
        };

        await codes.StoreAsync(code, ct);
        return code;
    }
}