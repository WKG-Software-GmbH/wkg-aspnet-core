namespace Wkg.AspNetCore.Authentication.Claims;

/// <summary>
/// Represents a verifiable claim that is used to identify a user.
/// </summary>
public abstract class IdentityClaim : Claim
{
    /// <summary>
    /// The subject of all identity claims.
    /// </summary>
    public static string IdentityClaimSubject => "Wkg.AspNetCore.Authentication.IdentityClaim";

    /// <summary>
    /// Creates a new instance of the <see cref="IdentityClaim"/> class.
    /// </summary>
    /// <param name="rawValue">The raw value of the identity claim. Must uniquely identify the user.</param>
    protected IdentityClaim(string rawValue) : base(IdentityClaimSubject, rawValue, requiresSerialization: false) => Pass();

    /// <summary>
    /// Creates a new instance of the <see cref="IdentityClaim"/> class.
    /// </summary>
    /// <param name="rawValue">The raw value of the identity claim. Must uniquely identify the user.</param>
    /// <param name="requiresSerialization">Whether the claim requires serialization.</param>
    protected IdentityClaim(string? rawValue, bool requiresSerialization) : base(IdentityClaimSubject, rawValue, requiresSerialization) => Pass();
}
