using System.Collections.Concurrent;

using Dais.Core.Domain;
using Dais.Core.Domain.Interfaces;

namespace Dais.Infrastructure.InMemory;

public class InMemoryAuthorizationCodeStore : IAuthorizationCodeStore
{
    private readonly ConcurrentDictionary<string, AuthorizationCode> _store = new();

    public Task StoreAsync(AuthorizationCode code, CancellationToken ct = default)
    {
        _store[code.Value] = code;
        return Task.CompletedTask;
    }

    public Task<AuthorizationCode?> FindAsync(string id, CancellationToken ct = default)
    {
        _store.TryGetValue(id, out var c);
        if (c is not null && (c.ExpiresAt <= DateTimeOffset.UtcNow || c.Consumed))
            c = null;
        return Task.FromResult(c);
    }

    public Task ConsumeAsync(string id, CancellationToken ct = default)
    {
        if (_store.TryGetValue(id, out var c))
            c.Consumed = true;
        return Task.CompletedTask;
    }
}