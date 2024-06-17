using System.Text.Json;
using System.Text.Json.Serialization;

namespace Wkg.AspNetCore.Authentication.Claims;

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

    protected internal override void Serialize()
    {
        RawValue = JsonSerializer.Serialize(_value);
        RequiresSerialization = false;
    }

    protected internal override void Deserialize() => _value = JsonSerializer.Deserialize<TValue>(RawValue)
        ?? throw new InvalidOperationException($"Failed to deserialize {nameof(Value)} from {RawValue}.");
}