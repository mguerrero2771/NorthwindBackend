using Microsoft.AspNetCore.Http;

namespace Microsoft.AspNetCore.Builder.Security.SessionBinding;

public record BoundSession(
    string SessionId,
    string UserId,
    string UserAgent,
    string? DeviceId,
    DateTime CreatedAtUtc,
    bool Revoked
);

public interface ISessionStore
{
    Task<string> CreateAsync(string userId, string userAgent, string? deviceId, CancellationToken ct = default);
    Task<BoundSession?> GetAsync(string sessionId, CancellationToken ct = default);
    Task RevokeAsync(string sessionId, CancellationToken ct = default);
    Task RevokeAllForUserAsync(string userId, CancellationToken ct = default);
}
