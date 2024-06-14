﻿using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Wkg.AspNetCore.Authentication;

public abstract class IdentityClaim(string value) : Claim(IdentityClaimSubject, value)
{
    public static string IdentityClaimSubject => "Wkg.AspNetCore.Authentication.IdentityClaim";
}

public class IdentityClaim<TIdentityKey> : IdentityClaim where TIdentityKey : notnull
{
    private TIdentityKey? _identityKey;

    public IdentityClaim(TIdentityKey value) : base(JsonSerializer.Serialize(value))
    {
        _identityKey = value;
    }

    [JsonConstructor]
    public IdentityClaim(string value) : base(value) => Pass();

    public TIdentityKey IdentityKey => _identityKey ??= JsonSerializer.Deserialize<TIdentityKey>(RawValue)
        ?? throw new InvalidOperationException($"Failed to deserialize {nameof(IdentityKey)} from {RawValue}.");
}

public class Claim(string subject, string value)
{
    public virtual string Subject { get; } = subject;

    public virtual string RawValue { get; protected set; } = value;

    internal Claim<TValue>? ToClaim<TValue>()
    {
        if (this is Claim<TValue> claim)
        {
            return claim;
        }
        TValue? value = JsonSerializer.Deserialize<TValue>(RawValue);
        return value is null ? null : new Claim<TValue>(Subject, RawValue, value);
    }
}

public class Claim<TValue> : Claim
{
    private TValue _value;

    public Claim(string subject, TValue value) : base(subject, JsonSerializer.Serialize(value))
    {
        _value = value;
    }

    internal Claim(string subject, string rawValue, TValue value) : base(subject, rawValue)
    {
        _value = value;
    }

    public TValue Value
    {
        get => _value;
        set
        {
            _value = value;
            RawValue = JsonSerializer.Serialize(value);
        }
    }
}
