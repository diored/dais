namespace Dais.Infrastructure.MongoDb.Dto;

internal class UserDto
{
    public required string Id { get; init; }
    public required string UserName { get; init; }
    public required string DisplayName { get; init; }
    public required string PasswordHash { get; init; }
}