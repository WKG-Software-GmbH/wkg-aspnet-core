using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;

namespace Wkg.AspNetCore.Authentication.Internals;

internal record SessionKeyStore(TimeSpan TimeToLive)
{
    private readonly ConcurrentDictionary<string, SessionKey> _sessionKeys = [];

    public bool TryGetSession(string sessionId, [NotNullWhen(true)] out SessionKey? sessionKey)
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

    public SessionKey GetOrCreateSession(string sessionId)
    {
        SessionKey sessionKey = _sessionKeys.GetOrAdd(sessionId, SessionKeyFactory);
        if (Interlocked.Read(ref sessionKey.CreatedAt) + TimeToLive.Ticks < DateTime.UtcNow.Ticks)
        {
            TryRevokeSession(sessionId);
            sessionKey = GetOrCreateSession(sessionId);
        }
        return sessionKey;
    }

    public bool TryRevokeSession(string sessionId) => _sessionKeys.TryRemove(sessionId, out _);

    private static SessionKey SessionKeyFactory(string _) => new();
}

internal record SessionKey
{
    private readonly Guid _key = Guid.NewGuid();
    private long _createdAt = DateTime.UtcNow.Ticks;

    public ref long CreatedAt => ref _createdAt;

    public int Size => 16;

    public void WriteKey(Span<byte> destination)
    {
        bool success = _key.TryWriteBytes(destination);
        if (!success)
        {
            throw new InvalidOperationException("Failed to retrieve session key.");
        }
    }
}