using Microsoft.Extensions.DependencyInjection;
using Wkg.AspNetCore.TestAdapters.Initialization;

namespace Wkg.AspNetCore.TestAdapters;

/// <summary>
/// Provides a base class for all unit tests that require dependency injection.
/// </summary>
/// <typeparam name="TInitializer">The <see cref="IDITestInitializer"/> implementation to be used for test initialization.</typeparam>
public abstract class DIAwareTestBase<TInitializer> : TestBase where TInitializer : IDITestInitializer
{
    private protected static ServiceProvider ServiceProvider { get; }

    static DIAwareTestBase()
    {
        // initialize DI
        ServiceCollection services = new();
        TInitializer.Configure(services);
        ServiceProvider = services.BuildServiceProvider();
        TInitializer.Initialize(ServiceProvider);
    }

    /// <summary>
    /// Executes the specified unit test in a dedicated DI scope.
    /// </summary>
    /// <param name="unitTestAction">The unit test to be executed.</param>
    protected async Task UsingServiceProviderAsync(Action<IServiceProvider> unitTestAction)
    {
        IServiceScopeFactory scopeFactory = ServiceProvider.GetRequiredService<IServiceScopeFactory>();
        await using AsyncServiceScope scope = scopeFactory.CreateAsyncScope();
        unitTestAction.Invoke(scope.ServiceProvider);
    }

    /// <summary>
    /// Executes the specified unit test asynchronously in a dedicated DI scope.
    /// </summary>
    /// <param name="unitTestTask">The unit test to be executed asynchronously.</param>
    protected async Task UsingServiceProviderAsync(Func<IServiceProvider, Task> unitTestTask)
    {
        IServiceScopeFactory scopeFactory = ServiceProvider.GetRequiredService<IServiceScopeFactory>();
        await using AsyncServiceScope scope = scopeFactory.CreateAsyncScope();
        await unitTestTask.Invoke(scope.ServiceProvider);
    }
}
