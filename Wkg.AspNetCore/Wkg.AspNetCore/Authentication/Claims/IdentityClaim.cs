namespace Wkg.AspNetCore.Authentication.Claims;

public abstract class IdentityClaim : Claim
{
    public static string IdentityClaimSubject => "Wkg.AspNetCore.Authentication.IdentityClaim";

    protected IdentityClaim(string rawValue) : base(IdentityClaimSubject, rawValue, requiresSerialization: false) => Pass();

    protected IdentityClaim(string? rawValue, bool requiresSerialization) : base(IdentityClaimSubject, rawValue, requiresSerialization) => Pass();
}
