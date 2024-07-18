using Microsoft.Extensions.DependencyInjection;

namespace Wkg.AspNetCore.ErrorHandling;

/// <summary>
/// Provides extension methods for configuring error handling services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the default <see cref="IErrorSentry"/> implementation for the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance for fluent configuration.</returns>
    public static IServiceCollection AddErrorHandling(this IServiceCollection services)
    {
        services.AddSingleton<IErrorSentry, DefaultErrorSentry>();
        return services;
    }
}
