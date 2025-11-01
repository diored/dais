using System.Security.Claims;

using DioRed.Dais.Core.Services;

using Microsoft.AspNetCore.Authentication;

namespace DioRed.Dais.ServerApp.Endpoints;

internal static class SetCookieEndpoint
{
    public static async Task<IResult> Handle(
        HttpContext ctx,
        ILoginContextService loginContextService,
        string code
    )
    {
        if (loginContextService.Pull(code) is not { } loginContext)
        {
            return Results.BadRequest("Invalid code");
        }

        ClaimsIdentity claimsIdentity = new(
            claims:
            [
                new Claim(ClaimTypes.NameIdentifier, loginContext.User.UserName),
                new Claim(ClaimTypes.Name, loginContext.User.DisplayName)
            ],
            authenticationType: "credentials"
        );

        await ctx.SignInAsync(
            "cookie",
            new ClaimsPrincipal(claimsIdentity)
        );

        return Results.Redirect(loginContext.AuthorizeCallback ?? "/");
    }
}