namespace DioRed.Dais.Core.Internal.Dto;

internal class ApplicationDto
{
    public required string Id { get; init; }
    public required string OwnerId { get; init; }
    public required string Name { get; init; }
    public required string[] Callbacks { get; init; }
}