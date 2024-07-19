using Microsoft.Extensions.DependencyInjection;
using Wkg.AspNetCore.Authentication.Jwt.Implementations.CookieBased;
using Wkg.AspNetCore.Authentication.Jwt.Internals;
using Wkg.AspNetCore.Authentication.Jwt.SigningFunctions;

namespace Wkg.AspNetCore.Authentication.Jwt;

/// <summary>
/// Builds the options for cookie claims.
/// </summary>
public class CookieClaimOptionsBuilder
{
    private TimeSpan _expiration = TimeSpan.FromHours(12);
    private bool _allowInsecure;
    private IJwtSigningFunctionBuilder? _signingFunction;

    /// <summary>
    /// Sets the expiration time for the cookie, default is 12 hours.
    /// </summary>
    public CookieClaimOptionsBuilder WithExpiration(TimeSpan expiration)
    {
        _expiration = expiration;
        return this;
    }

    /// <summary>
    /// Configures the signing function for the JWT token.
    /// </summary>
    /// <typeparam name="TSigningFunction">The type of the signing function.</typeparam>
    /// <param name="configureOptions">The action to configure the options for the signing function.</param>
    /// <returns>The current instance of the <see cref="CookieClaimOptionsBuilder"/>.</returns>
    public CookieClaimOptionsBuilder UseSigningFunction<TSigningFunction>(Action<TSigningFunction> configureOptions) 
        where TSigningFunction : class, IJwtSigningFunctionBuilder<TSigningFunction>
    {
        TSigningFunction signingFunction = TSigningFunction.Create();
        configureOptions(signingFunction);
        _signingFunction = signingFunction;
        return this;
    }

    /// <summary>
    /// Whether the cookie may be sent over an insecure (HTTP) connection, or whether TLS is required (HTTPS).
    /// </summary>
    /// <param name="allowInsecure"><see langword="true"/> to allow the cookie to be sent over an insecure connection; otherwise, <see langword="false"/>.</param>
    /// <returns>The current instance of the <see cref="CookieClaimOptionsBuilder"/>.</returns>
    public CookieClaimOptionsBuilder AllowInsecure(bool allowInsecure)
    {
        _allowInsecure = allowInsecure;
        return this;
    }

    internal CookieClaimOptions Build(IServiceCollection services)
    {
        if (_signingFunction is null)
        {
            throw new InvalidOperationException($"A signing function must be configured. Call {nameof(UseSigningFunction)} to set the signing function.");
        }
        _signingFunction.Build(services);
        return new CookieClaimOptions
        (
            !_allowInsecure,
            new ClaimValidationOptions(_expiration)
        );
    }
}
