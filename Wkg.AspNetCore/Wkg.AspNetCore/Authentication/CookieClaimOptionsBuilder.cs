using Wkg.AspNetCore.Authentication.Internals;

namespace Wkg.AspNetCore.Authentication;

/// <summary>
/// Builds the options for cookie claims.
/// </summary>
public class CookieClaimOptionsBuilder
{
    private TimeSpan _expiration = TimeSpan.FromHours(12);
    private string? _signingKey;

    /// <summary>
    /// Sets the expiration time for the cookie, default is 12 hours.
    /// </summary>
    public CookieClaimOptionsBuilder WithExpiration(TimeSpan expiration)
    {
        _expiration = expiration;
        return this;
    }

    /// <summary>
    /// Sets the signing key for the cookie.
    /// </summary>
    public CookieClaimOptionsBuilder WithSigningKey(string signingKey)
    {
        _signingKey = signingKey;
        return this;
    }

    internal ClaimValidationOptions Build() => new
    (
        _signingKey ?? throw new InvalidOperationException("The signing key is required."),
        _expiration
    );
}
