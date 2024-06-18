namespace Wkg.AspNetCore.Authentication.Internals;

internal class SessionKey<TExtendedKeys>(TExtendedKeys extendedKeys) : ISessionKey<TExtendedKeys> where TExtendedKeys : IExtendedKeys<TExtendedKeys>
{
    private readonly Guid _key = Guid.NewGuid();
    private long _createdAt = DateTime.UtcNow.Ticks;

    public TExtendedKeys ExtendedKeys { get; } = extendedKeys;

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
