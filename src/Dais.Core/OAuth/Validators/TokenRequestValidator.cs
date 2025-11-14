using Dais.Core.Domain;
using Dais.Core.OAuth.Models;

namespace Dais.Core.OAuth.Validators;

public sealed class TokenRequestValidator
{
    private static readonly HashSet<string> SupportedGrantTypes = new(StringComparer.Ordinal)
        { "authorization_code", "refresh_token" };

    public ValidationResult Validate(TokenRequest r, RegisteredClient? client, AuthorizationCode? code)
    {
        if (string.IsNullOrEmpty(r.GrantType))
            return Invalid(OAuthErrorCode.InvalidRequest, "grant_type is required");

        if (!SupportedGrantTypes.Contains(r.GrantType))
            return Invalid(OAuthErrorCode.UnsupportedGrantType, $"unsupported grant_type '{r.GrantType}'");

        if (client is null)
            return Invalid(OAuthErrorCode.InvalidClient, "unknown client_id");

        if (!client.AllowedGrantTypes.Contains(r.GrantType))
            return Invalid(OAuthErrorCode.UnauthorizedClient, "grant_type not allowed for this client");

        if (r.GrantType == "authorization_code")
        {
            if (string.IsNullOrEmpty(r.Code))
                return Invalid(OAuthErrorCode.InvalidGrant, "code missing");

            if (code is null)
                return Invalid(OAuthErrorCode.InvalidGrant, "authorization code not found or expired");

            if (r.GrantType != "refresh_token")
            {
                if (string.IsNullOrEmpty(r.RedirectUri))
                    return Invalid(OAuthErrorCode.InvalidGrant, "redirect_uri missing");

                if (!string.Equals(code.RedirectUri, r.RedirectUri, StringComparison.Ordinal))
                    return Invalid(OAuthErrorCode.InvalidGrant, "redirect_uri mismatch");
            }

            if (client.IsPublic || client.RequirePkce)
            {
                if (string.IsNullOrEmpty(r.CodeVerifier))
                    return Invalid(OAuthErrorCode.InvalidGrant, "code_verifier missing");

                if (!Pkce.IsValidVerifierFormat(r.CodeVerifier))
                    return Invalid(OAuthErrorCode.InvalidCodeVerifier, "code_verifier format/length invalid");

                var computed = Pkce.ComputeS256(r.CodeVerifier);
                if (!string.Equals(computed, code.CodeChallenge, StringComparison.Ordinal))
                    return Invalid(OAuthErrorCode.InvalidCodeVerifier, "code_verifier mismatch");
            }
        }

        return ValidationResult.Valid();
    }

    private static ValidationResult Invalid(OAuthErrorCode code, string desc, string? state = null)
        => ValidationResult.Invalid(code, desc, state);
}