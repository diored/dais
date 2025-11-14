using System.Security.Claims;

using Microsoft.AspNetCore.Authentication;

namespace Dais.ServerApp.Endpoints;

public static class AuthLoginCommitEndpoint
{
    public static async Task Handle(HttpContext ctx)
    {
        IFormCollection form = await ctx.Request.ReadFormAsync();
        string userName = form["userName"].ToString();
        string displayName = form["displayName"].ToString();

        Claim[] claims =
        [
            new Claim(ClaimTypes.NameIdentifier, userName),
            new Claim(ClaimTypes.Name, displayName)
        ];

        await ctx.SignInAsync(
            "cookie",
            new ClaimsPrincipal(new ClaimsIdentity(claims, "cookie"))
        );

        ctx.Response.StatusCode = 200;
    }
}