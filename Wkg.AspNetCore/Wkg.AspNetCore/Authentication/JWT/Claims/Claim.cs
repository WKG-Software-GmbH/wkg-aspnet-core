using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Wkg.AspNetCore.Authentication.Claims;

/// <summary>
/// Represents a verifiable key-value pair, used to claim a subject related to an authenticated entity.
/// </summary>
public partial class Claim
{
    /// <summary>
    /// The subject of the claim.
    /// </summary>
    public virtual string Subject { get; }

    /// <summary>
    /// The raw value of the claim. This value may be stored in a serialized format.
    /// </summary>
    [DisallowNull]
    public virtual string? RawValue { get; set; }

    /// <summary>
    /// Indicates whether the claim requires serialization.
    /// </summary>
    [MemberNotNullWhen(false, nameof(RawValue))]
    internal protected bool RequiresSerialization { get; protected set; }

    [JsonConstructor]
    internal Claim(string subject, string rawValue)
    {
        Subject = subject;
        RawValue = rawValue;
        RequiresSerialization = false;
    }

    /// <summary>
    /// Creates a new instance of the <see cref="Claim"/> class.
    /// </summary>
    /// <param name="subject">The subject of the claim.</param>
    /// <param name="rawValue">The raw value of the claim.</param>
    /// <param name="requiresSerialization">Whether the claim requires serialization.</param>
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

    /// <summary>
    /// Serializes the claim, ensuring that <see cref="RawValue"/> is set to a non-null value, and that <see cref="RequiresSerialization"/> is set to <see langword="false"/>.
    /// </summary>
    [MemberNotNull(nameof(RawValue))]
#pragma warning disable CS8774 // Member must have a non-null value when exiting.
    // justification: This method is overridden in derived classes. Concrete instances of this class always have a non-null value.
    protected internal virtual void Serialize() => RequiresSerialization = false;
#pragma warning restore CS8774 // Member must have a non-null value when exiting.

    /// <summary>
    /// Deserializes the claim from <see cref="RawValue"/>.
    /// </summary>
    protected internal virtual void Deserialize() => Pass();
}