using Microsoft.Extensions.DependencyInjection;
using Wkg.AspNetCore.Authentication.Jwt.Claims;
using Wkg.AspNetCore.Authentication.Jwt.Implementations.CookieBased;
using Wkg.AspNetCore.Authentication.Jwt.Internals;

namespace Wkg.AspNetCore.Authentication.Jwt;

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
    public static IServiceCollection AddJwtClaims<TIdentityClaim>(this IServiceCollection services, Action<CookieClaimOptionsBuilder> configureOptions)
        where TIdentityClaim : IdentityClaim
    {
        CookieClaimOptionsBuilder builder = new();
        configureOptions(builder);
        CookieClaimOptions options = builder.Build(services);
        services.AddHttpContextAccessor();
        services.AddSingleton(options);
        services.AddSingleton(options.ValidationOptions);
        services.AddSingleton(new SessionKeyStore<NoDecryptionKeys>(options.ValidationOptions.TimeToLive));
        services.AddScoped<IClaimManager<TIdentityClaim>, CookieClaimManager<TIdentityClaim, NoDecryptionKeys>>();
        services.AddScoped<IClaimRepository<TIdentityClaim>, CookieClaimRepository<TIdentityClaim, NoDecryptionKeys>>();
        return services;
    }

    /// <summary>
    /// Registers the services required for cookie-based claim management with decryption keys.
    /// </summary>
    /// <typeparam name="TIdentityClaim">The type of the identity claim.</typeparam>
    /// <typeparam name="TDecryptionKeys">The type of the decryption keys.</typeparam>
    /// <param name="services">The service collection to add the services to.</param>
    /// <param name="configureOptions">A delegate that configures the options for cookie-based claims.</param>
    /// <returns>The service collection with the added services.</returns>
    public static IServiceCollection AddJwtClaims<TIdentityClaim, TDecryptionKeys>(this IServiceCollection services, Action<CookieClaimOptionsBuilder> configureOptions)
        where TIdentityClaim : IdentityClaim
        where TDecryptionKeys : IDecryptionKeys<TDecryptionKeys>
    {
        CookieClaimOptionsBuilder builder = new();
        configureOptions(builder);
        CookieClaimOptions options = builder.Build(services);
        services.AddHttpContextAccessor();
        services.AddSingleton(options);
        services.AddSingleton(options.ValidationOptions);
        services.AddSingleton(new SessionKeyStore<TDecryptionKeys>(options.ValidationOptions.TimeToLive));
        services.AddScoped<IClaimManager<TIdentityClaim>, CookieClaimManager<TIdentityClaim, TDecryptionKeys>>();
        services.AddScoped<IClaimManager<TIdentityClaim, TDecryptionKeys>, CookieClaimManager<TIdentityClaim, TDecryptionKeys>>();
        services.AddScoped<IClaimRepository<TIdentityClaim>, CookieClaimRepository<TIdentityClaim, TDecryptionKeys>>();
        services.AddScoped<IClaimRepository<TIdentityClaim, TDecryptionKeys>, CookieClaimRepository<TIdentityClaim, TDecryptionKeys>>();
        return services;
    }
}
