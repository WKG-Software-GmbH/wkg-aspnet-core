﻿using Microsoft.Extensions.DependencyInjection;
using Wkg.AspNetCore.Authentication.Claims;
using Wkg.AspNetCore.Authentication.CookieBased;
using Wkg.AspNetCore.Authentication.Internals;

namespace Wkg.AspNetCore.Authentication;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCookieClaims<TIdentityClaim>(this IServiceCollection services, Action<CookieClaimOptionsBuilder> configureOptions)
        where TIdentityClaim : IdentityClaim
    {
        CookieClaimOptionsBuilder builder = new();
        configureOptions(builder);
        ClaimValidationOptions options = builder.Build();
        services.AddHttpContextAccessor();
        services.AddSingleton(options);
        services.AddSingleton(new SessionKeyStore(options.TimeToLive));
        services.AddScoped<IClaimManager<TIdentityClaim>, CookieClaimManager<TIdentityClaim>>();
        services.AddScoped<IClaimRepository<TIdentityClaim>, CookieClaimRepository<TIdentityClaim>>();
        return services;
    }
}

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