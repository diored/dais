using Microsoft.AspNetCore.Http;

namespace Dais.Core.OAuth;

public sealed class OAuthBearerErrorResult(
    OAuthErrorCode code,
    string? description,
    string? scope
) : IResult
{
    public async Task ExecuteAsync(HttpContext context)
    {
        var err = OAuthError.FromCode(code, description);
        var header = $"Bearer error=\"{err.Error}\"" +
                     (description is not null ? $", error_description=\"{description}\"" : "") +
                     (scope is not null ? $", scope=\"{scope}\"" : "");

        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        context.Response.ContentType = "application/json";
        context.Response.Headers.Append("WWW-Authenticate", header);

        await context.Response.WriteAsJsonAsync(err);
    }
}