namespace DioRed.Dais.Core.Dto;

internal class UserDto
{
    public required string Id { get; init; }
    public required string UserName { get; init; }
    public required string DisplayName { get; init; }
    public required string Password { get; init; }
    public required string Salt { get; init; }
}