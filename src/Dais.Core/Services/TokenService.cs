using System.Security.Claims;

using Dais.Core.Domain;
using Dais.Core.Domain.Interfaces;
using Dais.Core.Security;

using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace Dais.Core.Services;

public class TokenService(
    IAuthorizationCodeStore codes,
    ITokenStore tokens,
    IUserService users,
    DevKeys devKeys
) : ITokenService
{
    public async Task<AuthorizationCode?> FindAuthorizationCodeAsync(
        string? code,
        CancellationToken ct = default
    )
    {
        if (string.IsNullOrEmpty(code)) return null;
        return await codes.FindAsync(code, ct);
    }

    public async Task<TokenResponse> IssueTokensAsync(
        RegisteredClient client,
        AuthorizationCode code,
        string? codeVerifier,
        string? scope,
        CancellationToken ct = default)
    {
        await codes.ConsumeAsync(code.Value, ct);

        string subjectId = code.SubjectId
            ?? throw new InvalidOperationException("SubjectId missing in code");

        UserProfile user = users.GetByUsername(subjectId)
            ?? throw new InvalidOperationException("User not found");

        Dictionary<string, object> claims = new()
        {
            [JwtRegisteredClaimNames.Sub] = subjectId,
            [ClaimTypes.NameIdentifier] = subjectId,
            [ClaimTypes.Name] = user.DisplayName
        };

        if (!string.IsNullOrEmpty(scope))
        {
            claims["scope"] = scope;
        }

        DateTime expires = DateTime.UtcNow.AddMinutes(60);

        SecurityTokenDescriptor descriptor = new()
        {
            Claims = claims,
            Expires = expires,
            TokenType = "Bearer",
            SigningCredentials = new SigningCredentials(
                devKeys.RsaSecurityKey!,
                SecurityAlgorithms.RsaSha256
            )
        };

        JsonWebTokenHandler handler = new();
        string jwt = handler.CreateToken(descriptor);

        await tokens.StoreAccessTokenAsync(jwt, expires, client.ClientId, subjectId, scope, ct);

        return new TokenResponse
        {
            AccessToken = jwt,
            ExpiresIn = (int)(expires - DateTime.UtcNow).TotalSeconds,
            RefreshToken = null,
            Scope = scope
        };
    }
}