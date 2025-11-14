using Dais.Core.Domain.Interfaces;

namespace Dais.ServerApp.Endpoints;

public static class AuthLoginCheckEndpoint
{
    public static async Task<IResult> Handle(HttpContext ctx, IUserService users)
    {
        IFormCollection form = await ctx.Request.ReadFormAsync();
        string username = form["username"].ToString();
        string password = form["password"].ToString();

        UserInfo? user = users.ValidateCredentials(username, password);
        if (user is null)
        {
            return Results.Json(new { success = false });
        }

        return Results.Json(new
        {
            success = true,
            userName = user.UserName,
            displayName = user.DisplayName
        });
    }
}