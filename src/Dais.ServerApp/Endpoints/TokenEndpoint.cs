using System.Collections.Specialized;
using System.IO.Pipelines;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Web;

using DioRed.Dais.Core;
using DioRed.Dais.Core.Entities;
using DioRed.Dais.Core.Services;

using Microsoft.AspNetCore.DataProtection;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace DioRed.Dais.ServerApp.Endpoints;

internal static class TokenEndpoint
{
    public static async Task<IResult> Handle(
        HttpContext ctx,
        DevKeys devKeys,
        IDataProtectionProvider dataProtectionProvider,
        IDataService dataService
    )
    {
        ReadResult bodyBytes = await ctx.Request.BodyReader.ReadAsync();
        string bodyContent = Encoding.UTF8.GetString(bodyBytes.Buffer);

        NameValueCollection query = HttpUtility.ParseQueryString(bodyContent);

        TokenRequest request = TokenRequest.ParseFrom(query);

        if (string.IsNullOrEmpty(request.ClientId))
        {
            return BadRequest("client_id should be provided");
        }

        if (string.IsNullOrEmpty(request.ClientSecret))
        {
            return BadRequest("client_secret should be provided");
        }

        if (string.IsNullOrEmpty(request.GrantType))
        {
            return BadRequest("grant_type should be proveded");
        }

        if (string.IsNullOrEmpty(request.Code))
        {
            return BadRequest("code should be provided");
        }

        if (string.IsNullOrEmpty(request.RedirectUri))
        {
            return BadRequest("redirect_uri should be provided");
        }

        if (string.IsNullOrEmpty(request.CodeVerifier))
        {
            return BadRequest("code_verifier should be provided");
        }

        if (!dataService.HasRegisteredClient(request.ClientId, request.ClientSecret))
        {
            return BadRequest("Invalid client credentials");
        }

        if (request.GrantType != "authorization_code")
        {
            return BadRequest($"Unsupported grant_type: {request.GrantType}");
        }

        if (dataService.FindApplicationByCallback(request.RedirectUri, request.ClientId) is null)
        {
            return BadRequest($"Callback {request.RedirectUri} wasn't registered for this client");
        }

        var protector = dataProtectionProvider.CreateProtector("oauth");
        var codeString = protector.Unprotect(request.Code);

        AuthCode? authCode = JsonSerializer.Deserialize<AuthCode>(codeString);

        if (authCode is null)
        {
            return BadRequest("Cannot properly deserialize the code");
        }

        string codeChallenge = Base64UrlEncoder.Encode(
            SHA256.HashData(
                Encoding.ASCII.GetBytes(
                    request.CodeVerifier
                )
            )
        );

        if (authCode.CodeChallenge != codeChallenge)
        {
            return BadRequest("Invalid code");
        }

        JsonWebTokenHandler handler = new();

        string accessToken = handler.CreateToken(new SecurityTokenDescriptor()
        {
            Claims = new Dictionary<string, object>
            {
                [JwtRegisteredClaimNames.Sub] = authCode.UserName,
                [ClaimTypes.NameIdentifier] = authCode.UserName,
                [ClaimTypes.Name] = authCode.DisplayName
            },
            Expires = DateTime.Now.Add(Constants.AccessTokenExpiration),
            TokenType = "Bearer",
            SigningCredentials = new SigningCredentials(
                devKeys.RsaSecurityKey,
                SecurityAlgorithms.RsaSha256
            )
        });

        return Results.Ok(new
        {
            access_token = accessToken,
            token_type = "Bearer",
            expires_in = 900
        });

        static IResult BadRequest(string error)
        {
            return Results.BadRequest(new
            {
                error = $"invalid_request: {error}",
                iss = Constants.TokenIssuer
            });
        }
    }

    private record TokenRequest(
        string? ClientId,
        string? ClientSecret,
        string? GrantType,
        string? Code,
        string? RedirectUri,
        string? CodeVerifier
    )
    {
        public static TokenRequest ParseFrom(NameValueCollection query)
        {
            return new TokenRequest(
                query["client_id"],
                query["client_secret"],
                query["grant_type"],
                query["code"],
                query["redirect_uri"],
                query["code_verifier"]
            );
        }
    }
}