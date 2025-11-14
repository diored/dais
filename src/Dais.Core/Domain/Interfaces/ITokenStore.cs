namespace Dais.Core.Domain.Interfaces;

public interface ITokenStore
{
    Task StoreAccessTokenAsync(string token, DateTimeOffset expiresAt, string clientId, string? subjectId, string? scope, CancellationToken ct = default);
    Task<bool> IsAccessTokenActiveAsync(string token, CancellationToken ct = default);
}