using Microsoft.AspNetCore.Authentication;

namespace Dais.ServerApp.Endpoints;

internal static class AuthLogoutEndpoint
{
    public static async Task Handle(HttpContext ctx)
    {
        var returnUrl = ctx.Request.Query["returnUrl"].ToString();
        if (string.IsNullOrWhiteSpace(returnUrl))
            returnUrl = "/login";

        await ctx.SignOutAsync("cookie");

        ctx.Response.Redirect(returnUrl);
    }
}