using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using Wkg.Logging;

namespace Wkg.AspNetCore.Authentication.Jwt.Internals;

internal record SessionKeyStore<TDecryptionKeys>(TimeSpan TimeToLive) where TDecryptionKeys : IDecryptionKeys<TDecryptionKeys>
{
    private long _lastHousekeeping = DateTime.UtcNow.Ticks;

    private readonly ConcurrentDictionary<string, SessionKey<TDecryptionKeys>> _sessionKeys = [];

    public bool TryGetSession(string sessionId, [NotNullWhen(true)] out SessionKey<TDecryptionKeys>? sessionKey)
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

    public SessionKey<TDecryptionKeys> GetOrCreateSession(string sessionId, TDecryptionKeys decryptionKeys)
    {
        SessionKey<TDecryptionKeys> sessionKey = _sessionKeys.GetOrAdd(sessionId, _ => new SessionKey<TDecryptionKeys>(decryptionKeys));
        if (Interlocked.Read(ref sessionKey.CreatedAt) + TimeToLive.Ticks < DateTime.UtcNow.Ticks)
        {
            _sessionKeys.TryRemove(sessionId, out _);
            sessionKey = GetOrCreateSession(sessionId, decryptionKeys);
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
        foreach (KeyValuePair<string, SessionKey<TDecryptionKeys>> session in _sessionKeys)
        {
            if (Interlocked.Read(ref session.Value.CreatedAt) + TimeToLive.Ticks < DateTime.UtcNow.Ticks)
            {
                _sessionKeys.TryRemove(session.Key, out _);
            }
        }
    }
}
