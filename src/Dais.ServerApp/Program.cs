using DioRed.Dais.Core.Services;
using DioRed.Dais.ServerApp;
using DioRed.Dais.ServerApp.Components;
using DioRed.Dais.ServerApp.Endpoints;

using Microsoft.AspNetCore.HttpOverrides;

using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;

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

SetupMongoDb();

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

void SetupMongoDb()
{
    var conventionPack = new ConventionPack
    {
        new CamelCaseElementNameConvention(),
        new IgnoreIfNullConvention(true)
    };
    ConventionRegistry.Register("CamelCase", conventionPack, _ => true);

    string connectionString = builder.Configuration.GetRequiredValue("Database:ConnectionString");
    string databaseName = builder.Configuration.GetRequiredValue("Database:DatabaseName");
    string applicationsCollectionName = builder.Configuration.GetRequiredValue("Database:Collections:Applications");
    string clientsCollectionName = builder.Configuration.GetRequiredValue("Database:Collections:Clients");
    string usersCollectionName = builder.Configuration.GetRequiredValue("Database:Collections:Users");

    MongoClient mongoClient = new($"{connectionString}/?authSource={databaseName}");
    IMongoDatabase database = mongoClient.GetDatabase(databaseName);

    MongoDbDataService dataService = new(database, applicationsCollectionName, clientsCollectionName, usersCollectionName);

    builder.Services.AddSingleton<IDataService>(dataService);
}