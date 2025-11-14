using System.Text;

using Dais.Core.Domain;
using Dais.Core.Security;

using Microsoft.AspNetCore.Http;

namespace Dais.Core.OAuth;

public interface IClientAuthenticator
{
    Task<ClientAuthResult> AuthenticateAsync(HttpContext ctx, RegisteredClient? client);
}

public record ClientAuthResult(bool Success, OAuthErrorCode? Error = null, string? Description = null);

public sealed class ClientAuthenticator(
    IPasswordHasher hasher
) : IClientAuthenticator
{
    public async Task<ClientAuthResult> AuthenticateAsync(HttpContext ctx, RegisteredClient? client)
    {
        if (client is null)
            return new(false, OAuthErrorCode.InvalidClient, "unknown client");

        if (client.IsPublic)
            return new(true);

        // Basic
        if (ctx.Request.Headers.TryGetValue("Authorization", out var auth))
        {
            var parts = auth.ToString().Split(' ', 2);
            if (parts.Length == 2 && parts[0].Equals("Basic", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(parts[1]));
                    var pair = decoded.Split(':', 2);
                    if (pair.Length == 2)
                    {
                        if (!string.Equals(pair[0], client.ClientId, StringComparison.Ordinal))
                            return new(false, OAuthErrorCode.InvalidClient, "client_id mismatch");

                        if (client.SecretHash is not null && hasher.Verify(pair[1], client.SecretHash))
                            return new(true);
                    }
                }
                catch { }

                return new(false, OAuthErrorCode.InvalidClient, "invalid basic credentials");
            }
        }

        // Form fallback
        if (ctx.Request.HasFormContentType)
        {
            var form = await ctx.Request.ReadFormAsync();
            var id = form["client_id"].ToString();
            var secret = form["client_secret"].ToString();

            if (!string.Equals(id, client.ClientId, StringComparison.Ordinal))
                return new(false, OAuthErrorCode.InvalidClient, "client_id mismatch");

            if (client.SecretHash is not null && hasher.Verify(secret, client.SecretHash))
                return new(true);

            return new(false, OAuthErrorCode.InvalidClient, "invalid client_secret");
        }

        return new(false, OAuthErrorCode.InvalidClient, "missing client authentication");
    }
}