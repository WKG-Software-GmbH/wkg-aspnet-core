using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;

namespace Wkg.AspNetCore.TestAdapters;

public abstract class TestBase
{
    private static readonly object _lock = new();

    private protected static ServiceProvider ServiceProvider { get; private set; } = null!;

    protected TestBase()
    {
        EnsureIsInitialized();
    }

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
