namespace Dais.Infrastructure.MongoDb.Dto;

internal class ClientDto
{
    public required string Id { get; init; }
    public required string OwnerId { get; init; }
    public required string ClientId { get; init; }
    public required string ClientSecretHash { get; init; }
    public required string DisplayName { get; init; }
    public required string[] Callbacks { get; init; }
}