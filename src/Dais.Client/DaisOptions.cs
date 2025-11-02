using Microsoft.AspNetCore.Authentication.Cookies;

namespace DioRed.Dais.Client;

public class DaisOptions
{
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string SignInScheme { get; set; } = CookieAuthenticationDefaults.AuthenticationScheme;
    public string CallbackPath { get; set; } = DaisDefaults.CallbackPath;
}