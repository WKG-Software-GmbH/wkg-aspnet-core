using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace Wkg.AspNetCore.Authentication.Internals;

internal record SessionKeyStore<TExtendedKeys>(TimeSpan TimeToLive) where TExtendedKeys : IExtendedKeys<TExtendedKeys>
{
    private readonly ConcurrentDictionary<string, SessionKey<TExtendedKeys>> _sessionKeys = [];

    public bool TryGetSession(string sessionId, [NotNullWhen(true)] out SessionKey<TExtendedKeys>? sessionKey)
    {
        if (!_sessionKeys.TryGetValue(sessionId, out sessionKey))
        {
            goto FAILURE;
        }
        if (Interlocked.Read(ref sessionKey.CreatedAt) + TimeToLive.Ticks < DateTime.UtcNow.Ticks)
        {
            TryRevokeSession(sessionId);
            goto FAILURE;
        }
        return true;
    FAILURE:
        sessionKey = null;
        return false;
    }

    public SessionKey<TExtendedKeys> GetOrCreateSession(string sessionId, TExtendedKeys extendedKeys)
    {
        SessionKey<TExtendedKeys> sessionKey = _sessionKeys.GetOrAdd(sessionId, _ => new SessionKey<TExtendedKeys>(extendedKeys));
        if (Interlocked.Read(ref sessionKey.CreatedAt) + TimeToLive.Ticks < DateTime.UtcNow.Ticks)
        {
            TryRevokeSession(sessionId);
            sessionKey = GetOrCreateSession(sessionId, extendedKeys);
        }
        return sessionKey;
    }

    public bool TryRevokeSession(string sessionId) => _sessionKeys.TryRemove(sessionId, out _);
}
