namespace Wkg.AspNetCore.Authentication.Jwt;

/// <summary>
/// Represents a session key.
/// </summary>
public interface ISessionKey
{
    /// <summary>
    /// Retrieves a reference to the creation timestamp of the session key.
    /// This timestamp is used to calculate the expiry date of the session and must be updated atomically and in a thread-safe manner.
    /// </summary>
    ref long CreatedAt { get; }

    /// <summary>
    /// The size in bytes of the session key. 
    /// Must be a constant value and must be small enough for stack allocation.
    /// </summary>
    int Size { get; }

    /// <summary>
    /// Writes the session key to the specified destination.
    /// </summary>
    /// <param name="destination">The destination span to write the session key to.</param>
    void WriteKey(Span<byte> destination);
}

/// <summary>
/// Represents a session key with support for user-supplied decryption key data.
/// </summary>
/// <typeparam name="TDecryptionKeys">The type of the decryption key data.</typeparam>
public interface ISessionKey<TDecryptionKeys> : ISessionKey where TDecryptionKeys : IDecryptionKeys<TDecryptionKeys>
{
    /// <summary>
    /// The user-supplied decryption key data.
    /// </summary>
    TDecryptionKeys DecryptionKeys { get; }
}