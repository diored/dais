using System.Text;
using System.Text.Json.Serialization;

namespace Dais.Core.OAuth;

public record OAuthError(
    [property: JsonPropertyName("error")] string Error,
    [property: JsonPropertyName("error_description")] string? Description = null,
    [property: JsonPropertyName("error_uri")] string? ErrorUri = null,
    [property: JsonPropertyName("state")] string? State = null)
{
    public static OAuthError FromCode(
        OAuthErrorCode code,
        string? description = null,
        string? state = null,
        string? errorUri = null
    )
    {
        return new OAuthError(
            ToSnakeCase(code.ToString()),
            description,
            errorUri,
            state
        );
    }

    private static string ToSnakeCase(string s)
    {
        StringBuilder sb = new();

        for (int i = 0; i < s.Length; i++)
        {
            char ch = s[i];
            if (i > 0 && char.IsUpper(ch))
            {
                sb.Append('_');
            }
            sb.Append(char.ToLowerInvariant(ch));
        }

        return sb.ToString();
    }
}