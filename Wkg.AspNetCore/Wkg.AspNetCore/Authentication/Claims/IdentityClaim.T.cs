using System.Text.Json;
using System.Text.Json.Serialization;

namespace Wkg.AspNetCore.Authentication.Claims;

public class IdentityClaim<TIdentityKey> : IdentityClaim where TIdentityKey : notnull
{
    private TIdentityKey? _identityKey;

    public IdentityClaim(TIdentityKey value) : base(rawValue: null, requiresSerialization: true)
    {
        _identityKey = value;
    }

    [JsonConstructor]
    public IdentityClaim(string rawValue) : base(rawValue, requiresSerialization: false) => Pass();

    [JsonIgnore]
    public TIdentityKey IdentityKey => _identityKey
        ?? throw new InvalidOperationException($"Unable to retrieve {nameof(IdentityKey)} from {nameof(IdentityClaim)} in this context. Ensure the {nameof(IdentityClaim)} is correctly initialized.");

    protected internal override void Serialize() => RawValue = JsonSerializer.Serialize(_identityKey);

    protected internal override void Deserialize() => _identityKey = JsonSerializer.Deserialize<TIdentityKey>(RawValue)
        ?? throw new InvalidOperationException($"Failed to deserialize {nameof(IdentityKey)} from {RawValue}.");
}
