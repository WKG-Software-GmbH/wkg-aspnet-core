using System.Text.Json;
using System.Text.Json.Serialization;

namespace Wkg.AspNetCore.Authentication.Jwt.Claims;

/// <summary>
/// Represents a verifiable, strongly-typed key-value pair, used to claim a subject related to an authenticated entity.
/// </summary>
/// <typeparam name="TValue">The type of the value.</typeparam>
public class Claim<TValue> : Claim
{
    private TValue _value;

    internal Claim(string subject, TValue value) : base(subject, rawValue: null, requiresSerialization: true)
    {
        _value = value;
    }

    internal Claim(string subject, string rawValue, TValue value, bool requiresSerialization) : base(subject, rawValue, requiresSerialization)
    {
        _value = value;
    }

    /// <summary>
    /// The value of the claim.
    /// </summary>
    [JsonIgnore]
    public TValue Value
    {
        get => _value;
        set
        {
            _value = value;
            RequiresSerialization = true;
        }
    }

    /// <inheritdoc />
    protected internal override void Serialize()
    {
        RawValue = JsonSerializer.Serialize(_value);
        RequiresSerialization = false;
    }

    /// <inheritdoc />
    protected internal override void Deserialize()
    {
        _ = RawValue ?? throw new InvalidOperationException($"Failed to deserialize {nameof(Claim)} from null-value.");
        _value = JsonSerializer.Deserialize<TValue>(RawValue)
            ?? throw new InvalidOperationException($"Failed to deserialize {nameof(Value)} from {RawValue}.");
    }
}