using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Wkg.AspNetCore.Authentication.Claims;

public partial class Claim
{
    public virtual string Subject { get; }

    public virtual string RawValue { get; set; }

    [MemberNotNullWhen(false, nameof(RawValue))]
    internal protected bool RequiresSerialization { get; protected set; }

    [JsonConstructor]
    internal Claim(string subject, string rawValue) => (Subject, RawValue) = (subject, rawValue);

    internal protected Claim(string subject, string? rawValue, bool requiresSerialization)
    {
        Subject = subject;
        RawValue = rawValue!;
        RequiresSerialization = requiresSerialization;
    }

    internal Claim<TValue>? ToClaim<TValue>()
    {
        if (this is Claim<TValue> claim)
        {
            return claim;
        }
        TValue? value = JsonSerializer.Deserialize<TValue>(RawValue!);
        return value is null ? null : new Claim<TValue>(Subject, RawValue!, value, requiresSerialization: false);
    }

    [MemberNotNull(nameof(RawValue))]
#pragma warning disable CS8774 // Member must have a non-null value when exiting.
    // justification: This method is overridden in derived classes. Concrete instances of this class always have a non-null value.
    protected internal virtual void Serialize() => RequiresSerialization = false;
#pragma warning restore CS8774 // Member must have a non-null value when exiting.

    protected internal virtual void Deserialize() => Pass();
}