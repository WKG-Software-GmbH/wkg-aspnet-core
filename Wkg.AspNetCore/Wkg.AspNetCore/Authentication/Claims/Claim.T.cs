using System.Text.Json.Serialization;

namespace Wkg.AspNetCore.Authentication.Claims;

public class Claim<TValue> : Claim
{
    private TValue _value;

    public Claim(string subject, TValue value) : base(subject, rawValue: null, requiresSerialization: true)
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
}