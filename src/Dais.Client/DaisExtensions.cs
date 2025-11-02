using System.Text.Json;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DioRed.Dais.Client;

public static class DaisExtensions
{
    extension(AuthenticationBuilder builder)
    {
        public AuthenticationBuilder AddDais(Action<DaisOptions> configureOptions)
        {
            return builder.AddDais(DaisDefaults.AuthenticationScheme, configureOptions);
        }

        public AuthenticationBuilder AddDais(
            string authenticationScheme,
            Action<DaisOptions> configureOptions
        )
        {
            DaisOptions daisOptions = new();
            configureOptions(daisOptions);

            if (string.IsNullOrWhiteSpace(daisOptions.ClientId))
            {
                throw new InvalidOperationException("ClientId should be set");
            }

            if (string.IsNullOrWhiteSpace(daisOptions.ClientSecret))
            {
                throw new InvalidOperationException("ClientSecret should be set");
            }

            return builder.AddOAuth(authenticationScheme, o =>
            {
                o.ClientId = daisOptions.ClientId;
                o.ClientSecret = daisOptions.ClientSecret;

                o.SignInScheme = daisOptions.SignInScheme;
                o.CallbackPath = daisOptions.CallbackPath;

                o.AuthorizationEndpoint = "https://dais.dio.red/oauth/authorize";
                o.TokenEndpoint = "https://dais.dio.red/oauth/token";

                o.UsePkce = true;
                o.ClaimActions.MapAll();
                o.Events.OnCreatingTicket = async ctx =>
                {
                    var payloadBase64 = ctx.AccessToken!.Split('.')[1];
                    var payloadJson = Base64UrlTextEncoder.Decode(payloadBase64);
                    var payload = JsonDocument.Parse(payloadJson);
                    ctx.RunClaimActions(payload.RootElement);
                };
            });
        }
    }

    extension(WebApplicationBuilder builder)
    {
        public AuthenticationBuilder AddDaisCookieAuthentication(
            string cookieAuthenticationScheme = CookieAuthenticationDefaults.AuthenticationScheme,
            string daisAuthenticationScheme = DaisDefaults.AuthenticationScheme,
            Action<CookieAuthenticationOptions>? configureCookieOptions = null,
            Action<DaisOptions>? configureDaisOptions = null
        )
        {
            AuthenticationBuilder authenticationBuilder = builder.Services.AddAuthentication(options =>
            {
                options.DefaultScheme = daisAuthenticationScheme;
                options.DefaultSignInScheme = cookieAuthenticationScheme;
            });

            if (configureCookieOptions is null)
            {
                authenticationBuilder.AddCookie(cookieAuthenticationScheme);
            }
            else
            {
                authenticationBuilder.AddCookie(cookieAuthenticationScheme, configureCookieOptions);
            }

            configureDaisOptions ??= new Action<DaisOptions>(o =>
            {
                o.ClientId = builder.Configuration.GetRequiredValue("Dais:ClientId");
                o.ClientSecret = builder.Configuration.GetRequiredValue("Dais:ClientSecret");
            });

            authenticationBuilder.AddDais(daisAuthenticationScheme, configureDaisOptions);

            return authenticationBuilder;
        }
    }
}