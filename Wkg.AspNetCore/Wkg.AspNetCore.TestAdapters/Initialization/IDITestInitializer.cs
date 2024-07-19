using Microsoft.Extensions.DependencyInjection;

namespace Wkg.AspNetCore.TestAdapters.Initialization;

/// <summary>
/// Represents dependency injection setup code that is executed before the first test of the first test class requiring DI is executed.
/// </summary>
public interface IDITestInitializer
{
    /// <summary>
    /// Configures the specified <paramref name="services"/> for dependency injection.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to configure.</param>
    static abstract void Configure(IServiceCollection services);

    /// <summary>
    /// Performs additional initialization steps after the DI container has been built.
    /// </summary>
    static abstract void Initialize(IServiceProvider serviceProvider);
}
