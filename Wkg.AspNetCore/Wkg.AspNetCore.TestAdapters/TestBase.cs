using Microsoft.Extensions.DependencyInjection;

namespace Wkg.AspNetCore.TestAdapters;

/// <summary>
/// Provides a base class for tests that require dependency injection to be configured.
/// </summary>
public abstract class TestBase
{
    private static readonly object _lock = new();

    private protected static ServiceProvider ServiceProvider { get; private set; } = null!;

    /// <summary>
    /// Initializes a new instance of the <see cref="TestBase"/> class.
    /// </summary>
    protected TestBase()
    {
        EnsureIsInitialized();
    }

    /// <summary>
    /// Creates the <see cref="ServiceProvider"/> that is used to resolve dependencies.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> that can be used to register dependencies.</param>
    protected abstract ServiceProvider CreateServiceProvider(IServiceCollection services);

    private protected virtual void OnInitialized() { }

    private void EnsureIsInitialized()
    {
        lock (_lock)
        {
            if (ServiceProvider is null)
            {
                // initialize once and only once
                IServiceCollection services = new ServiceCollection();
                ServiceProvider = CreateServiceProvider(services);
                OnInitialized();
            }
        }
    }
}
