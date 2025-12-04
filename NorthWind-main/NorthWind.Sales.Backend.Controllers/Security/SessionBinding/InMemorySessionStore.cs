using System.Collections.Concurrent;

namespace Microsoft.AspNetCore.Builder.Security.SessionBinding;

public sealed class InMemorySessionStore : ISessionStore
{
    private readonly ConcurrentDictionary<string, BoundSession> _bySession = new();
    private readonly ConcurrentDictionary<string, HashSet<string>> _byUser = new();

    public Task<string> CreateAsync(string userId, string userAgent, string? deviceId, CancellationToken ct = default)
    {
        var sid = Guid.NewGuid().ToString("N");
        var session = new BoundSession(sid, userId, userAgent ?? string.Empty, deviceId, DateTime.UtcNow, false);
        _bySession[sid] = session;
        _byUser.AddOrUpdate(userId, _ => new HashSet<string> { sid }, (_, set) => { set.Add(sid); return set; });
        return Task.FromResult(sid);
    }

    public Task<BoundSession?> GetAsync(string sessionId, CancellationToken ct = default)
    {
        _bySession.TryGetValue(sessionId, out var session);
        return Task.FromResult(session);
    }

    public Task RevokeAsync(string sessionId, CancellationToken ct = default)
    {
        if (_bySession.TryGetValue(sessionId, out var s))
        {
            _bySession[sessionId] = s with { Revoked = true };
        }
        return Task.CompletedTask;
    }

    public Task RevokeAllForUserAsync(string userId, CancellationToken ct = default)
    {
        if (_byUser.TryGetValue(userId, out var set))
        {
            foreach (var sid in set)
            {
                if (_bySession.TryGetValue(sid, out var s))
                {
                    _bySession[sid] = s with { Revoked = true };
                }
            }
        }
        return Task.CompletedTask;
    }
}
