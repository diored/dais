using System.Security.Claims;

using Dais.Core.Domain.Interfaces;

using Microsoft.AspNetCore.Authentication;

namespace Dais.ServerApp.Endpoints;

public static class AuthLoginEndpoint
{
    public static async Task<IResult> Handle(HttpContext ctx, IUserService users)
    {
        IFormCollection form = await ctx.Request.ReadFormAsync();
        string username = form["username"].ToString();
        string password = form["password"].ToString();
        string returnUrl = form["returnUrl"].ToString();

        if (string.IsNullOrWhiteSpace(returnUrl))
        {
            returnUrl = "/";
        }

        UserInfo? user = users.ValidateCredentials(username, password);
        if (user is null)
        {
            return Results.Unauthorized();
        }

        Claim[] claims =
        [
            new Claim(ClaimTypes.NameIdentifier, user.UserName),
            new Claim(ClaimTypes.Name, user.DisplayName)
        ];

        await ctx.SignInAsync(
            "cookie",
            new ClaimsPrincipal(new ClaimsIdentity(claims, "cookie"))
        );

        return Results.Redirect(returnUrl);
    }
}