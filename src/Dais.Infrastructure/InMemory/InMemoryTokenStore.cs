using System.Collections.Concurrent;

using Dais.Core.Domain.Interfaces;

namespace Dais.Infrastructure.InMemory;

public class InMemoryTokenStore : ITokenStore
{
    private readonly ConcurrentDictionary<string, DateTimeOffset> _tokens = new();

    public Task StoreAccessTokenAsync(string token, DateTimeOffset exp, string clientId, string? subjectId, string? scope, CancellationToken ct = default)
    {
        _tokens[token] = exp;
        return Task.CompletedTask;
    }

    public Task<bool> IsAccessTokenActiveAsync(string token, CancellationToken ct = default)
    {
        if (_tokens.TryGetValue(token, out var exp) && exp > DateTimeOffset.UtcNow)
            return Task.FromResult(true);
        return Task.FromResult(false);
    }
}