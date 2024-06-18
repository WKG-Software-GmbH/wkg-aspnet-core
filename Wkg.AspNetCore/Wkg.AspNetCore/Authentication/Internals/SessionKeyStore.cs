using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using Wkg.Logging;

namespace Wkg.AspNetCore.Authentication.Internals;

internal record SessionKeyStore<TExtendedKeys>(TimeSpan TimeToLive) where TExtendedKeys : IExtendedKeys<TExtendedKeys>
{
    private long _lastHousekeeping = DateTime.UtcNow.Ticks;

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
        TryRunHousekeeping();
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
            _sessionKeys.TryRemove(sessionId, out _);
            sessionKey = GetOrCreateSession(sessionId, extendedKeys);
        }
        TryRunHousekeeping();
        return sessionKey;
    }

    public bool TryRevokeSession(string sessionId)
    {
        bool result = _sessionKeys.TryRemove(sessionId, out _);
        TryRunHousekeeping();
        return result;
    }

    private void TryRunHousekeeping()
    {
        long lastHousekeeping = Interlocked.Read(ref _lastHousekeeping);
        if (lastHousekeeping + TimeToLive.Ticks < DateTime.UtcNow.Ticks && Interlocked.CompareExchange(ref _lastHousekeeping, DateTime.UtcNow.Ticks, lastHousekeeping) == lastHousekeeping)
        {
            Log.WriteDiagnostic("[SessionKeyStore] Releasing expired session keys.");
            RunHousekeeping();
        }
    }

    public void RunHousekeeping()
    {
        Interlocked.Exchange(ref _lastHousekeeping, DateTime.UtcNow.Ticks);
        foreach (KeyValuePair<string, SessionKey<TExtendedKeys>> session in _sessionKeys)
        {
            if (Interlocked.Read(ref session.Value.CreatedAt) + TimeToLive.Ticks < DateTime.UtcNow.Ticks)
            {
                _sessionKeys.TryRemove(session.Key, out _);
            }
        }
    }
}
