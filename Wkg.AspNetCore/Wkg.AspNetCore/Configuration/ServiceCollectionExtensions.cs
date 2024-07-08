using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Wkg.AspNetCore.Abstractions.Controllers;
using Wkg.AspNetCore.Abstractions.RazorPages;
using Wkg.AspNetCore.Transactions;

namespace Wkg.AspNetCore.Configuration;

/// <summary>
/// Extension methods for <see cref="IServiceCollection"/>.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Configures the <see cref="IServiceCollection"/> using the specified <typeparamref name="TStartupScript"/>.
    /// </summary>
    /// <typeparam name="TStartupScript">The type of the startup script.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/>.</param>
    /// <param name="configuration">
    /// The <see cref="IConfiguration"/>. 
    /// If no configuration is specified, creates a new configuration that expects
    /// the "appsettings.json" file to exist. Additionally, the configuration also
    /// reads from "appsettings.[my_asp_environment].json", should the file exist.
    /// </param>
    /// <returns>The <see cref="IServiceCollection"/> for fluent configuration.</returns>
    public static IServiceCollection ConfigureUsing<TStartupScript>(this IServiceCollection services, IConfiguration? configuration = null) where TStartupScript : IStartupScript
    {
        string env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
            ?? "Development";

        configuration ??= new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile($"appsettings.{env}.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        TStartupScript.ConfigureServices(services, configuration);
        return services;
    }
}