using DioRed.Dais.Core.Services;
using DioRed.Dais.ServerApp.Components;
using DioRed.Dais.ServerApp.Endpoints;
using DioRed.Dais.ServerApp.Internal;

using Microsoft.AspNetCore.HttpOverrides;

using MongoDB.Bson.Serialization.Conventions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication("cookie")
    .AddCookie("cookie", o =>
    {
        o.LoginPath = "/login";
        o.LogoutPath = "/logout";
    });
builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();

builder.Services.AddSingleton<DevKeys>();
builder.Services.AddSingleton<ILoginContextService, InMemoryLoginContextService>();

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
app.UseHttpsRedirection();

app.UseAntiforgery();

ForwardHeadersForHttps();

app.UseAuthorization();

app.MapGet("/oauth/authorize", AuthorizeEndpoint.Handle);
app.MapPost("/oauth/token", TokenEndpoint.Handle);
app.MapGet("/oauth/set-cookie", SetCookieEndpoint.Handle);
app.MapGet("/logout", LogoutEndpoint.Handle);
app.MapGet("/version", VersionEndpoint.Handle);

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
    MongoDbDataService dataService = new(settings);

    builder.Services.AddSingleton<IDataService>(dataService);
}