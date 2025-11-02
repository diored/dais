namespace DioRed.Dais.Core.Internal.Dto;

internal class ClientDto
{
    public required string Id { get; init; }
    public required string ClientId { get; init; }
    public required string Secret { get; init; }
    public required string Salt { get; init; }
}