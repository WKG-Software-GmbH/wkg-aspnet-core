using Microsoft.Extensions.DependencyInjection;
using Wkg.AspNetCore.Authentication.Jwt.SigningFunctions.Implementations;

namespace Wkg.AspNetCore.Authentication.Jwt.SigningFunctions;

/// <summary>
/// A builder for creating an HMAC SHA-256 signing function.
/// </summary>
public class HmacSha256 : IJwtSigningFunctionBuilder<HmacSha256>
{
    private string? _secret = null;

    static HmacSha256 IJwtSigningFunctionBuilder<HmacSha256>.Create() => new();

    /// <inheritdoc />
    public HmacSha256 UsingSecret(string secret)
    {
        if (_secret is not null)
        {
            throw new InvalidOperationException("The secret has already been set.");
        }
        ArgumentException.ThrowIfNullOrWhiteSpace(secret, nameof(secret));
        _secret = secret;
        return this;
    }

    void IJwtSigningFunctionBuilder.Build(IServiceCollection services)
    {
        if (string.IsNullOrWhiteSpace(_secret))
        {
            throw new ArgumentException("A secret must be set before building the HMAC SHA-256 signing function.");
        }
        services.AddSingleton(new HmacOptions(_secret));
        services.AddSingleton<IJwtSigningFunction, HmacSha256SigningFunction>();
    }
}
