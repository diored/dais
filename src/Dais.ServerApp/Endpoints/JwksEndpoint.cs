using Dais.Core;
using Dais.Core.Security;

using Microsoft.IdentityModel.Tokens;

namespace Dais.ServerApp.Endpoints;

public static class JwksEndpoint
{
    public static IResult Handle(DevKeys keys)
    {
        var rsa = keys.RsaKey.ExportParameters(false);

        var jwk = new Dictionary<string, object>
        {
            ["kty"] = "RSA",
            ["use"] = "sig",
            ["kid"] = keys.KeyId,
            ["alg"] = SecurityAlgorithms.RsaSha256,
            ["n"] = rsa.Modulus!.ToBase64Url(),
            ["e"] = rsa.Exponent!.ToBase64Url()
        };

        return Results.Json(new
        {
            keys = new[] { jwk }
        });
    }
}