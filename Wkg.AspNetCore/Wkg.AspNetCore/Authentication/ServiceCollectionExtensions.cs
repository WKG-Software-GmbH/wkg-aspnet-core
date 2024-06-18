using Microsoft.Extensions.DependencyInjection;
using Wkg.AspNetCore.Authentication.Claims;
using Wkg.AspNetCore.Authentication.CookieBased;
using Wkg.AspNetCore.Authentication.Internals;

namespace Wkg.AspNetCore.Authentication;

/// <summary>
/// Provides extension methods for adding cookie-based claims to the service collection.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the services required for cookie-based claim management.
    /// </summary>
    /// <typeparam name="TIdentityClaim">The type of the identity claim.</typeparam>
    /// <param name="services">The service collection to add the services to.</param>
    /// <param name="configureOptions">A delegate that configures the options for cookie-based claims.</param>
    /// <returns>The service collection with the added services.</returns>
    public static IServiceCollection AddCookieClaims<TIdentityClaim>(this IServiceCollection services, Action<CookieClaimOptionsBuilder> configureOptions)
        where TIdentityClaim : IdentityClaim
    {
        CookieClaimOptionsBuilder builder = new();
        configureOptions(builder);
        ClaimValidationOptions options = builder.Build();
        services.AddHttpContextAccessor();
        services.AddSingleton(options);
        services.AddSingleton(new SessionKeyStore<NoExtendedKeys>(options.TimeToLive));
        services.AddScoped<IClaimManager<TIdentityClaim>, CookieClaimManager<TIdentityClaim, NoExtendedKeys>>();
        services.AddScoped<IClaimRepository<TIdentityClaim>, CookieClaimRepository<TIdentityClaim, NoExtendedKeys>>();
        return services;
    }

    /// <summary>
    /// Registers the services required for cookie-based claim management with extended keys.
    /// </summary>
    /// <typeparam name="TIdentityClaim">The type of the identity claim.</typeparam>
    /// <typeparam name="TExtendedKeys">The type of the extended keys.</typeparam>
    /// <param name="services">The service collection to add the services to.</param>
    /// <param name="configureOptions">A delegate that configures the options for cookie-based claims.</param>
    /// <returns>The service collection with the added services.</returns>
    public static IServiceCollection AddCookieClaims<TIdentityClaim, TExtendedKeys>(this IServiceCollection services, Action<CookieClaimOptionsBuilder> configureOptions)
        where TIdentityClaim : IdentityClaim
        where TExtendedKeys : IExtendedKeys<TExtendedKeys>
    {
        CookieClaimOptionsBuilder builder = new();
        configureOptions(builder);
        ClaimValidationOptions options = builder.Build();
        services.AddHttpContextAccessor();
        services.AddSingleton(options);
        services.AddSingleton(new SessionKeyStore<TExtendedKeys>(options.TimeToLive));
        services.AddScoped<IClaimManager<TIdentityClaim>, CookieClaimManager<TIdentityClaim, TExtendedKeys>>();
        services.AddScoped<IClaimManager<TIdentityClaim, TExtendedKeys>, CookieClaimManager<TIdentityClaim, TExtendedKeys>>();
        services.AddScoped<IClaimRepository<TIdentityClaim>, CookieClaimRepository<TIdentityClaim, TExtendedKeys>>();
        services.AddScoped<IClaimRepository<TIdentityClaim, TExtendedKeys>, CookieClaimRepository<TIdentityClaim, TExtendedKeys>>();
        return services;
    }
}
