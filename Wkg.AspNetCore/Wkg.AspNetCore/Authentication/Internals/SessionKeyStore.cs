using System.Collections.Concurrent;

namespace Wkg.AspNetCore.Authentication.Internals;

internal record SessionKeyStore()
{
    public ConcurrentDictionary<string, Guid> SessionKeys { get; } = [];
}