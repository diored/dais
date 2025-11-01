using Microsoft.AspNetCore.Authentication;

namespace DioRed.Dais.ServerApp.Endpoints;

internal static class LogoutEndpoint
{
    public static async Task Handle(HttpContext ctx)
    {
        await ctx.SignOutAsync("cookie");
        ctx.Response.Redirect($"/login{ctx.Request.QueryString.Value}");
    }
}