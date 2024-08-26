using System.Text.Json;
using System.Text.Json.Serialization;

namespace Wkg.AspNetCore.Authentication.Jwt.Claims;

/// <summary>
/// Represents a verifiable, strongly-typed claim that is used to identify a user.
/// </summary>
/// <typeparam name="TIdentityKey">The type of the identity key.</typeparam>
public class IdentityClaim<TIdentityKey> : IdentityClaim where TIdentityKey : notnull
{
    private TIdentityKey? _identityKey;

    /// <summary>
    /// Creates a new instance of the <see cref="IdentityClaim{TIdentityKey}"/> class.
    /// </summary>
    /// <param name="value">The value of the identity claim. Must uniquely identify the user.</param>
    public IdentityClaim(TIdentityKey value) : base(rawValue: null, requiresSerialization: true)
    {
        _identityKey = value;
    }

    /// <summary>
    /// Creates a new instance of the <see cref="IdentityClaim{TIdentityKey}"/> class from a serialized value.
    /// </summary>
    /// <param name="rawValue">The serialized value of the identity claim. Must uniquely identify the user.</param>
    [JsonConstructor]
    public IdentityClaim(string rawValue) : base(rawValue, requiresSerialization: false)
    {
        Deserialize();
    }

    /// <summary>
    /// The identity key of the claim.
    /// </summary>
    [JsonIgnore]
    public TIdentityKey IdentityKey => _identityKey
        ?? throw new InvalidOperationException($"Unable to retrieve {nameof(IdentityKey)} from {nameof(IdentityClaim)} in this context. Ensure the {nameof(IdentityClaim)} is correctly initialized.");

    /// <inheritdoc />
    internal protected override void Serialize()
    {
        RawValue = JsonSerializer.Serialize(_identityKey);
        RequiresSerialization = false;
    }

    /// <inheritdoc />
    internal protected override void Deserialize()
    {
        _ = RawValue ?? throw new InvalidOperationException($"Failed to deserialize {nameof(IdentityClaim)} from null-value.");
        _identityKey = JsonSerializer.Deserialize<TIdentityKey>(RawValue)
            ?? throw new InvalidOperationException($"Failed to deserialize {nameof(IdentityKey)} from {RawValue}.");
    }
}
