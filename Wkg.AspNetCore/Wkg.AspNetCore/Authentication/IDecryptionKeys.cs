namespace Wkg.AspNetCore.Authentication;

/// <summary>
/// Represents an abstract structure that extends the key data of a session key with additional user-defined data.
/// Implementations of this interface should be immutable.
/// </summary>
/// <typeparam name="TSelf">The type of the implementing class or struct.</typeparam>
public interface IDecryptionKeys<TSelf> where TSelf : IDecryptionKeys<TSelf>
{
    /// <summary>
    /// Generates a new instance of the implementing class or struct and initializes it with cryptographically secure key data.
    /// </summary>
    /// <returns>A fully initialized instance of <typeparamref name="TSelf"/>.</returns>
    static abstract TSelf Generate();
}