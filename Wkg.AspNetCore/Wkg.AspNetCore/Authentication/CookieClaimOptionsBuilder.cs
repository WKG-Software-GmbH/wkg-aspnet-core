using Wkg.AspNetCore.Authentication.CookieBased;
using Wkg.AspNetCore.Authentication.Internals;

namespace Wkg.AspNetCore.Authentication;

/// <summary>
/// Builds the options for cookie claims.
/// </summary>
public class CookieClaimOptionsBuilder
{
    private TimeSpan _expiration = TimeSpan.FromHours(12);
    private string? _signingKey;
    private bool _allowInsecure;

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

    /// <summary>
    /// Whether the cookie may be sent over an insecure (HTTP) connection, or whether TLS is required (HTTPS).
    /// </summary>
    /// <param name="allowInsecure"><see langword="true"/> to allow the cookie to be sent over an insecure connection; otherwise, <see langword="false"/>.</param>"
    /// <returns>The current instance of the <see cref="CookieClaimOptionsBuilder"/>.</returns>
    public CookieClaimOptionsBuilder AllowInsecure(bool allowInsecure)
    {
        _allowInsecure = allowInsecure;
        return this;
    }

    internal CookieClaimOptions Build() => new
    (
        !_allowInsecure,
        new ClaimValidationOptions
        (
            _signingKey ?? throw new InvalidOperationException("Signing key must be set."), 
            _expiration
        )
    );
}
