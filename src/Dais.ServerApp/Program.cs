using Dais.Core.Domain.Interfaces;
using Dais.Core.OAuth.Validators;
using Dais.Core.Security;
using Dais.Core.Services;
using Dais.Infrastructure.InMemory;
using Dais.Infrastructure.MongoDb;
using Dais.ServerApp.Components;
using Dais.ServerApp.Endpoints;

using Microsoft.AspNetCore.HttpOverrides;

using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = "cookie";
    options.DefaultSignInScheme = "cookie";
    options.DefaultChallengeScheme = "cookie";
})
    .AddCookie("cookie", o =>
    {
        o.LoginPath = "/login";
        o.LogoutPath = "/logout";

        o.Cookie.Name = "dais";
#if DEBUG
        o.Cookie.SecurePolicy = CookieSecurePolicy.None;
#else
        o.Cookie.SecurePolicy = CookieSecurePolicy.Always;
#endif

        o.Cookie.HttpOnly = true;
        o.Cookie.SameSite = SameSiteMode.Lax;
        o.Cookie.Path = "/";
    });
builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();

builder.Services.AddSingleton<DevKeys>();
builder.Services.AddSingleton<IAuthorizationCodeStore, InMemoryAuthorizationCodeStore>();
builder.Services.AddSingleton<ITokenStore, InMemoryTokenStore>();
builder.Services.AddSingleton<IAuthorizationService, AuthorizationService>();
builder.Services.AddSingleton<ITokenService, TokenService>();
builder.Services.AddSingleton<AuthorizeRequestValidator>();
builder.Services.AddSingleton<TokenRequestValidator>();
builder.Services.AddSingleton<IPasswordHasher, Argon2PasswordHasher>();

SetupDataService(builder);

builder.Services.AddHttpContextAccessor();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
//app.UseHttpsRedirection();

app.UseAntiforgery();

ForwardHeadersForHttps();

app.UseAuthentication();
app.UseAuthorization();

// --- OAuth protocol endpoints ---
app.MapGet("/oauth/authorize", AuthorizeEndpoint.Handle);
app.MapPost("/oauth/token", TokenEndpoint.Handle);
app.MapGet("/oauth/userinfo", UserInfoEndpoint.Handle);

// --- Well-known metadata ---
app.MapGet("/.well-known/openid-configuration", DiscoveryEndpoint.Handle);
app.MapGet("/jwks.json", (DevKeys keys) => JwksEndpoint.Handle(keys));

// --- Auth backend endpoints ---
app.MapPost("/auth/login", AuthLoginEndpoint.Handle);
app.MapPost("/auth/login-check", AuthLoginCheckEndpoint.Handle);
app.MapPost("/auth/login-commit", AuthLoginCommitEndpoint.Handle);
app.MapPost("/auth/logout", AuthLogoutEndpoint.Handle);

// UI logout page → calls backend logout
app.MapGet("/logout", () => Results.Redirect("/auth/logout"));

app.MapGet("/version", VersionEndpoint.Handle);

app.MapGet("/debug-auth", (HttpContext ctx) =>
{
    return ctx.User.Identity?.IsAuthenticated == true
        ? Results.Text($"AUTHENTICATED as {ctx.User.Identity.Name}")
        : Results.Text("ANONYMOUS");
});

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
return;

void ForwardHeadersForHttps()
{
    ForwardedHeadersOptions forwardedHeadersOptions = new()
    {
        ForwardedHeaders = ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost
    };

    forwardedHeadersOptions.KnownIPNetworks.Clear();
    forwardedHeadersOptions.KnownProxies.Clear();

    app.UseForwardedHeaders(forwardedHeadersOptions);
}

static void SetupDataService(WebApplicationBuilder builder)
{
    ConventionPack conventionPack =
    [
        new CamelCaseElementNameConvention(),
        new IgnoreIfNullConvention(true)
    ];
    ConventionRegistry.Register("CamelCase", conventionPack, _ => true);

    MongoDbSettings settings = builder.Configuration.GetRequiredSection("Database").GetRequired<MongoDbSettings>();

    MongoClient mongoClient = new($"{settings.ConnectionString}/?authSource={settings.DatabaseName}");
    IMongoDatabase database = mongoClient.GetDatabase(settings.DatabaseName);

    builder.Services.AddSingleton<IClientService, MongoDbClientService>(sp => new MongoDbClientService(
        database,
        settings.Collections.Clients,
        sp.GetRequiredService<IPasswordHasher>()
    ));

    builder.Services.AddSingleton<IUserService, MongoDbUserService>(sp => new MongoDbUserService(
        database,
        settings.Collections.Users,
        sp.GetRequiredService<IPasswordHasher>()
    ));
}