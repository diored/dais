using Dais.Core;
using Dais.Core.Domain.Interfaces;
using Dais.Core.OAuth;
using Dais.Core.OAuth.Models;
using Dais.Core.OAuth.Validators;

namespace Dais.ServerApp.Endpoints;

internal static class TokenEndpoint
{
    public static async Task<IResult> Handle(
        HttpContext ctx,
        IClientService clients,
        IClientAuthenticator authenticator,
        ITokenService tokens,
        TokenRequestValidator validator)
    {
        var form = await ctx.Request.ReadFormAsync();
        var req = new TokenRequest
        {
            GrantType = form["grant_type"],
            ClientId = form["client_id"],
            ClientSecret = form["client_secret"],
            RedirectUri = form["redirect_uri"],
            Code = form["code"],
            CodeVerifier = form["code_verifier"],
            RefreshToken = form["refresh_token"],
            Scope = form["scope"]
        };

        var client = req.ClientId is null ? null : clients.FindById(req.ClientId);
        var auth = await authenticator.AuthenticateAsync(ctx, client);
        if (!auth.Success)
            return OAuthErrorFactory.JsonError(auth.Error!.Value, auth.Description);

        var code = await tokens.FindAuthorizationCodeAsync(req.Code);
        var validation = validator.Validate(req, client!, code);
        if (!validation.IsValid)
            return OAuthErrorFactory.JsonError(validation.ErrorCode!.Value, validation.Description, validation.State);

        var response = await tokens.IssueTokensAsync(client!, code!, req.CodeVerifier, req.Scope);
        return Results.Json(response)
            .WithHeader("Cache-Control", "no-store");
    }
}