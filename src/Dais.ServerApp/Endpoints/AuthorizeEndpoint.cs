using System.Security.Claims;

using Dais.Core.Domain.Interfaces;
using Dais.Core.OAuth;
using Dais.Core.OAuth.Models;
using Dais.Core.OAuth.Validators;

namespace Dais.ServerApp.Endpoints;

internal static class AuthorizeEndpoint
{
    public static async Task<IResult> Handle(
        HttpContext ctx,
        IClientService clients,
        IAuthorizationService authService,
        AuthorizeRequestValidator validator)
    {
        var q = ctx.Request.Query;

        var req = new AuthorizeRequest
        {
            ResponseType = q["response_type"],
            ClientId = q["client_id"],
            RedirectUri = q["redirect_uri"],
            Scope = q["scope"],
            State = q["state"],
            CodeChallenge = q["code_challenge"],
            CodeChallengeMethod = q["code_challenge_method"]
        };

        var client = req.ClientId is null ? null : clients.FindById(req.ClientId);
        var validation = validator.Validate(req, client);

        if (!validation.IsValid)
        {
            if (string.IsNullOrEmpty(req.RedirectUri))
                return OAuthErrorFactory.JsonError(validation.ErrorCode!.Value, validation.Description, validation.State);

            return OAuthErrorFactory.RedirectError(req.RedirectUri!, validation.ErrorCode!.Value, validation.Description, validation.State);
        }

        // если не залогинен — на login
        if (!ctx.User.Identity?.IsAuthenticated ?? true)
        {
            var returnUrl = ctx.Request.Path + ctx.Request.QueryString;
            return Results.Redirect($"/login?returnUrl={Uri.EscapeDataString(returnUrl)}");
        }

        // consent (Confirm) — если confirm=yes, выдаём код
        if (q.TryGetValue("confirm", out var confirmFlag) && confirmFlag == "yes")
        {
            var subjectId = ctx.User.FindFirstValue(ClaimTypes.NameIdentifier)!;

            if (client!.RequirePkce && string.IsNullOrEmpty(req.CodeChallenge))
            {
                return OAuthErrorFactory.RedirectError(
                    req.RedirectUri!,
                    OAuthErrorCode.InvalidRequest,
                    "code_challenge required for this client",
                    req.State
                );
            }

            var code = await authService.CreateAuthorizationCodeAsync(
                client!,
                req.RedirectUri!,
                req.CodeChallenge!,
                subjectId,
                req.Scope);

            var redirect = $"{req.RedirectUri}?code={Uri.EscapeDataString(code.Value)}";
            if (req.State is not null)
                redirect += $"&state={Uri.EscapeDataString(req.State)}";

            return Results.Redirect(redirect);
        }

        // иначе — на Confirm
        var fullAuthorizeUrl = ctx.Request.Path + ctx.Request.QueryString;
        return Results.Redirect($"/confirm?returnUrl={Uri.EscapeDataString(fullAuthorizeUrl)}");
    }
}