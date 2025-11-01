using System.Collections.Concurrent;

using DioRed.Dais.Core.Entities;

namespace DioRed.Dais.Core.Services;

public class InMemoryLoginContextService : ILoginContextService
{
    private readonly ConcurrentDictionary<string, LoginContext> _cache = [];

    public string Save(LoginContext loginContext)
    {
        string key = Guid.NewGuid().ToString();
        _cache[key] = loginContext;

        return key;
    }

    public LoginContext? Pull(string key)
    {
        if (_cache.TryGetValue(key, out LoginContext? loginContext))
        {
            // One-time
            _cache.Remove(key, out _);

            // Remove outdated
            if (DateTimeOffset.Now > loginContext.ExpiresAt)
            {
                loginContext = null;
            }
        }

        return loginContext;
    }
}