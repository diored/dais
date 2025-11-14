using Microsoft.AspNetCore.Http;

namespace Dais.Core.OAuth;

public static class OAuthErrorFactory
{
    public static IResult JsonError(
        OAuthErrorCode code,
        string? description = null,
        string? state = null,
        string? errorUri = null,
        int statusCode = StatusCodes.Status400BadRequest
    )
    {
        var payload = OAuthError.FromCode(code, description, state, errorUri);

        return Results.Json(payload, statusCode: statusCode)
            .WithHeader("Cache-Control", "no-store");
    }

    public static IResult RedirectError(
        string redirectUri,
        OAuthErrorCode code,
        string? description = null,
        string? state = null,
        string? errorUri = null)
    {
        var err = OAuthError.FromCode(code, description, state, errorUri);

        var qs = new QueryString()
            .Add("error", err.Error)
            .AddIfNotEmpty("error_description", err.Description)
            .AddIfNotEmpty("state", err.State)
            .AddIfNotEmpty("error_uri", err.ErrorUri);

        return Results.Redirect($"{redirectUri}{qs}");
    }

    public static IResult BearerError(OAuthErrorCode code, string? description = null, string? scope = null)
    {
        var err = OAuthError.FromCode(code, description);

        var header = new AuthenticateHeader()
            .Add("Bearer error", err.Error)
            .AddIfNotEmpty("error_description", err.Description)
            .AddIfNotEmpty("scope", scope);

        return Results.Json(err, statusCode: StatusCodes.Status401Unauthorized)
            .WithHeader("WWW-Authenticate", header.ToString())
            .WithHeader("Cache-Control", "no-store");
    }
}