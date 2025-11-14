using System.Security.Claims;

using Dais.Core.Domain;
using Dais.Core.Domain.Interfaces;
using Dais.Core.Security;

using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace Dais.Core.Services;

public class TokenService(
    IAuthorizationCodeStore codes,
    ITokenStore tokens,
    IUserService users,
    DevKeys devKeys,
    IConfiguration config
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
            ?? throw new InvalidOperationException("SubjectId missing in authorization code");

        UserProfile user = users.GetByUsername(subjectId)
            ?? throw new InvalidOperationException("User not found");

        string issuer = config["Dais:Issuer"]
            ?? throw new InvalidOperationException("Issuer not defined in config.");

        Dictionary<string, object> claims = new()
        {
            [JwtRegisteredClaimNames.Sub] = subjectId,
            [ClaimTypes.NameIdentifier] = subjectId,
            [ClaimTypes.Name] = user.DisplayName,
            ["client_id"] = client.ClientId,
            ["scope"] = scope ?? ""
        };

        DateTime now = DateTime.UtcNow;
        DateTime expires = now.AddMinutes(60);

        SigningCredentials signingCredentials = new(
            devKeys.RsaSecurityKey,
            SecurityAlgorithms.RsaSha256
        );

        SecurityTokenDescriptor descriptor = new()
        {
            Issuer = issuer,
            Audience = client.ClientId,
            Subject = new ClaimsIdentity(
                claims.Select(c => new Claim(c.Key, c.Value.ToString()!))
            ),
            Expires = expires,
            NotBefore = now,
            IssuedAt = now,
            SigningCredentials = signingCredentials,
            TokenType = "at+jwt"
        };

        JsonWebTokenHandler handler = new();

        string jwt = handler.CreateToken(descriptor);

        await tokens.StoreAccessTokenAsync(jwt, expires, client.ClientId, subjectId, scope, ct);

        return new TokenResponse
        {
            AccessToken = jwt,
            ExpiresIn = (int)(expires - now).TotalSeconds,
            RefreshToken = null,
            Scope = scope
        };
    }
}