namespace DioRed.Dais.Core.Entities;

public class RegisteredClientWithCallbacks
{
    public required string DisplayName { get; init; }
    public required string[] Callbacks { get; init; }
}