using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Wkg.AspNetCore.Configuration;

/// <summary>
/// Represents a startup script that configures a web application. Basically a replacement for the <see cref="IStartup"/> implementation, just static and simpler.
/// </summary>
public interface IStartupScript
{
    /// <summary>
    /// Configures the specified <paramref name="app"/>.
    /// </summary>
    /// <param name="app">The <see cref="WebApplication"/> to configure.</param>
    static abstract void Configure(WebApplication app);

    /// <summary>
    /// Configures the specified <paramref name="services"/> for dependency injection.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to configure.</param>
    /// <param name="configuration">The <see cref="IConfiguration"/>.</param>
    static abstract void ConfigureServices(IServiceCollection services, IConfiguration configuration);
}