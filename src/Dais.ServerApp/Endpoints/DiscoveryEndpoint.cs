namespace Dais.ServerApp.Endpoints;

public static class DiscoveryEndpoint
{
    private static readonly string[] _reponseTypesSupported = ["code"];
    private static readonly string[] _subjectTypesSupported = ["public"];
    private static readonly string[] _idTokenSigningAlgValuesSupported = ["RS256"];
    private static readonly string[] _tokenEndpointAuthMethodsSupported = ["client_secret_post", "client_secret_basic", "none"];
    private static readonly string[] _scopesSupported = ["openid", "profile", "email"];

    public static IResult Handle(HttpContext ctx)
    {
        var issuer = $"{ctx.Request.Scheme}://{ctx.Request.Host}";

        return Results.Json(new
        {
            issuer,
            authorization_endpoint = $"{issuer}/oauth/authorize",
            token_endpoint = $"{issuer}/oauth/token",
            userinfo_endpoint = $"{issuer}/oauth/userinfo",
            jwks_uri = $"{issuer}/jwks.json",
            response_types_supported = _reponseTypesSupported,
            subject_types_supported = _subjectTypesSupported,
            id_token_signing_alg_values_supported = _idTokenSigningAlgValuesSupported,
            token_endpoint_auth_methods_supported = _tokenEndpointAuthMethodsSupported,
            scopes_supported = _scopesSupported
        });
    }
}