using System.Security.Claims;

using Dais.Core;
using Dais.Core.OAuth;
using Dais.Core.Security;

using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace Dais.ServerApp.Endpoints;

public static class UserInfoEndpoint
{
    public static async Task<IResult> Handle(
        HttpContext ctx,
        DevKeys keys
    )
    {
        var auth = ctx.Request.Headers.Authorization.ToString();
        if (!auth.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            return OAuthErrorFactory.BearerError(OAuthErrorCode.InvalidToken, "missing bearer token");

        var token = auth["Bearer ".Length..].Trim();

        var handler = new JsonWebTokenHandler();

        var result = await handler.ValidateTokenAsync(token, new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            RequireSignedTokens = true,
            IssuerSigningKey = keys.RsaSecurityKey,
            ValidateIssuerSigningKey = true
        });

        if (!result.IsValid)
        {
            return OAuthErrorFactory.BearerError(OAuthErrorCode.InvalidToken, result.Exception?.Message);
        }

        var jwt = (JsonWebToken)result.SecurityToken;

        var sub = jwt.GetPayloadValue<string>("sub");
        var name = jwt.GetPayloadValue<string>(ClaimTypes.Name);
        var preferred = jwt.GetPayloadValue<string>(ClaimTypes.NameIdentifier);

        return Results.Json(new
        {
            sub,
            name,
            preferred_username = preferred
        }).WithHeader("Cache-Control", "no-store");
    }
}