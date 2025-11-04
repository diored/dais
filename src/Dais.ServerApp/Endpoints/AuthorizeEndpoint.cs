using System.Security.Claims;
using System.Text.Json;
using System.Web;

using DioRed.Dais.Core;
using DioRed.Dais.Core.Entities;
using DioRed.Dais.Core.Services;

using Microsoft.AspNetCore.DataProtection;

namespace DioRed.Dais.ServerApp.Endpoints;

internal static class AuthorizeEndpoint
{
    public static IResult Handle(
        HttpContext ctx,
        IDataProtectionProvider dataProtectionProvider,
        ILoginContextService loginContextService,
        IDataService dataService
    )
    {
        var authorizeCallback = $"/oauth/authorize{ctx.Request.QueryString.Value}";

        if (!ctx.User?.Identity?.IsAuthenticated ?? true)
        {
            return Results.Redirect($"/login?authorizeCallback={HttpUtility.UrlEncode(authorizeCallback)}");
        }

        AuthorizeRequest request = AuthorizeRequest.ParseFrom(ctx.Request.Query);

        if (string.IsNullOrEmpty(request.ResponseType))
        {
            return BadRequest("response_type was not provided");
        }

        if (string.IsNullOrEmpty(request.ClientId))
        {
            return BadRequest("client_id was not provided");
        }

        if (string.IsNullOrEmpty(request.CodeChallenge))
        {
            return BadRequest("code_challenge was not provided");
        }

        if (string.IsNullOrEmpty(request.CodeChallengeMethod))
        {
            return BadRequest("code_challenge_method was not provided");
        }

        if (string.IsNullOrEmpty(request.RedirectUri))
        {
            return BadRequest("redirect_uri was not provided");
        }

        if (request.ResponseType != "code")
        {
            return BadRequest($"Unsuppored response_type: {request.ResponseType}");
        }

        if (request.CodeChallengeMethod != "S256")
        {
            return BadRequest($"Unsupported code_challenge_method: {request.CodeChallengeMethod}");
        }

        RegisteredClient? client = dataService.FindClient(request.ClientId);
        if (client is null)
        {
            return BadRequest("Invalid client id");
        }

        IDataProtector protector = dataProtectionProvider.CreateProtector("oauth");

        AuthCode authCode = new()
        {
            ClientId = request.ClientId ?? "",
            CodeChallenge = request.CodeChallenge ?? "",
            CodeChallengeMethod = request.CodeChallengeMethod ?? "",
            RedirectUri = request.RedirectUri ?? "",
            Expiry = DateTime.Now.Add(Constants.AuthCodeExpiration),
            UserName = ctx.User?.FindFirstValue(ClaimTypes.NameIdentifier) ?? "",
            DisplayName = ctx.User?.FindFirstValue(ClaimTypes.Name) ?? "",
            ApplicationName = client.DisplayName
        };

        string authCodeString = protector.Protect(JsonSerializer.Serialize(authCode));

        string returnUrl = $"{request.RedirectUri}?code={authCodeString}&state={request.State}&iss={Constants.UrlEncodedTokenIssuer}";

        LoginContext loginContext = new()
        {
            User = new LoginContext.UserData(
                authCode.UserName,
                authCode.DisplayName
            ),
            AuthorizeCallback = authorizeCallback,
            Application = new LoginContext.ApplicationData(
                client.DisplayName,
                returnUrl
            ),
            ExpiresAt = DateTimeOffset.Now.Add(Constants.LoginContextExpiration)
        };

        string key = loginContextService.Save(loginContext);

        return Results.Redirect($"/confirm?code={key}");

        IResult BadRequest(string error)
        {
            return Results.BadRequest(new
            {
                error = $"invalid_request: {error}",
                state = request.State,
                iss = Constants.UrlEncodedTokenIssuer
            });
        }
    }

    private record AuthorizeRequest(
        string? State,
        string? ResponseType,
        string? ClientId,
        string? CodeChallenge,
        string? CodeChallengeMethod,
        string? RedirectUri,
        string? Scope
    )
    {
        public static AuthorizeRequest ParseFrom(IQueryCollection query)
        {
            return new AuthorizeRequest(
                query["state"],
                query["response_type"],
                query["client_id"],
                query["code_challenge"],
                query["code_challenge_method"],
                query["redirect_uri"],
                query["scope"]
            );
        }
    }
}