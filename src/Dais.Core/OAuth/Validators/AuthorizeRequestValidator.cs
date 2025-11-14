using Dais.Core.Domain;
using Dais.Core.OAuth.Models;

namespace Dais.Core.OAuth.Validators;

public sealed class AuthorizeRequestValidator
{
    private static readonly HashSet<string> SupportedResponseTypes = new(StringComparer.Ordinal) { "code" };

    public ValidationResult Validate(AuthorizeRequest r, RegisteredClient? client)
    {
        if (string.IsNullOrEmpty(r.ResponseType))
            return Invalid(OAuthErrorCode.InvalidRequest, "response_type is required", r.State);

        if (!SupportedResponseTypes.Contains(r.ResponseType))
            return Invalid(OAuthErrorCode.UnsupportedResponseType, $"unsupported response_type '{r.ResponseType}'", r.State);

        if (string.IsNullOrEmpty(r.ClientId))
            return Invalid(OAuthErrorCode.InvalidRequest, "client_id is required", r.State);

        if (client is null)
            return Invalid(OAuthErrorCode.InvalidClient, "unknown client_id", r.State);

        if (string.IsNullOrEmpty(r.RedirectUri))
            return Invalid(OAuthErrorCode.InvalidRequest, "redirect_uri is required", r.State);

        if (!client.RedirectUris.Contains(r.RedirectUri))
            return Invalid(OAuthErrorCode.InvalidRedirectUri, "unregistered redirect_uri", r.State);

        if (!string.IsNullOrEmpty(r.Scope))
        {
            foreach (var s in r.Scope.Split(' ', StringSplitOptions.RemoveEmptyEntries))
                if (!client.AllowedScopes.Contains(s))
                    return Invalid(OAuthErrorCode.InvalidScope, $"scope '{s}' is not allowed", r.State);
        }

        if (client.IsPublic || client.RequirePkce)
        {
            if (string.IsNullOrEmpty(r.CodeChallenge))
                return Invalid(OAuthErrorCode.InvalidRequest, "code_challenge is required for PKCE", r.State);
            if (!"S256".Equals(r.CodeChallengeMethod, StringComparison.OrdinalIgnoreCase))
                return Invalid(OAuthErrorCode.InvalidRequest, "code_challenge_method must be 'S256'", r.State);
            if (r.CodeChallenge.Length is < 43 or > 128)
                return Invalid(OAuthErrorCode.InvalidCodeChallenge, "code_challenge length invalid", r.State);
        }

        return ValidationResult.Valid();
    }

    private static ValidationResult Invalid(OAuthErrorCode code, string desc, string? state)
        => ValidationResult.Invalid(code, desc, state);
}