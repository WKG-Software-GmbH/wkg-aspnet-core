namespace Wkg.AspNetCore.Authentication.Internals;

internal class SessionKey<TDecryptionKeys>(TDecryptionKeys decryptionKeys) : ISessionKey<TDecryptionKeys> where TDecryptionKeys : IDecryptionKeys<TDecryptionKeys>
{
    private readonly Guid _key = Guid.NewGuid();
    private long _createdAt = DateTime.UtcNow.Ticks;

    public TDecryptionKeys DecryptionKeys { get; } = decryptionKeys;

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
