using System.Web;

namespace DioRed.Dais.Core;

public class Constants
{
    public const string TokenIssuer = "https://dais.dio.red";
    public static string UrlEncodedTokenIssuer { get; } = HttpUtility.UrlEncode(TokenIssuer);

    public static TimeSpan LoginContextExpiration { get; } = TimeSpan.FromMinutes(5);
    public static TimeSpan AuthCodeExpiration { get; } = TimeSpan.FromMinutes(5);
    public static TimeSpan AccessTokenExpiration { get; } = TimeSpan.FromMinutes(15);
}