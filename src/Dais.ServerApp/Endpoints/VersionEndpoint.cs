namespace DioRed.Dais.ServerApp.Endpoints;

public static class VersionEndpoint
{
    public static IResult Handle(IConfiguration configuration)
    {
        return Results.Text(configuration["Version"]);
    }
}