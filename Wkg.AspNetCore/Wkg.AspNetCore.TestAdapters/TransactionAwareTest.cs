using Microsoft.Extensions.DependencyInjection;
using Wkg.AspNetCore.TestAdapters.Extensions;

namespace Wkg.AspNetCore.TestAdapters;

/// <summary>
/// Represents the base class for transaction-aware tests of ASP.NET Core components.
/// </summary>
/// <typeparam name="TComponent">The type of the component to be tested. Must be activatable using DI.</typeparam>
/// <typeparam name="TDatabaseLoader">The type of the <see cref="IDatabaseLoader"/> to be used to initialize the database, if necessary.</typeparam>
public abstract class TransactionAwareTest<TComponent, TDatabaseLoader> : TestBase
    where TComponent : class
    where TDatabaseLoader : IDatabaseLoader
{
    private protected TransactionAwareTest() => Pass();

    private protected override void OnInitialized()
    {
        base.OnInitialized();
        TDatabaseLoader.InitializeDatabase(ServiceProvider);
    }

    /// <summary>
    /// Executes the specified unit test against the component in a transactional context.
    /// </summary>
    /// <param name="unitTest">The unit test to be executed.</param>
    /// <remarks>
    /// Any transactional context created by the component will be rolled back after the unit test has been executed.
    /// </remarks>
    private protected virtual async Task ActivateAndRunAsync(Action<TComponent> unitTest)
    {
        IServiceScopeFactory scopeFactory = ServiceProvider.GetRequiredService<IServiceScopeFactory>();
        await using AsyncServiceScope scope = scopeFactory.CreateAsyncScope();
        TComponent component = scope.ServiceProvider.Activate<TComponent>();
        unitTest.Invoke(component);
    }

    /// <summary>
    /// Executes the specified unit test asynchronously against the component in a transactional context.
    /// </summary>
    /// <param name="unitTestAsync">The unit test to be executed asynchronously.</param>
    /// <remarks>
    /// Any transactional context created by the component will be rolled back after the unit test has been executed.
    /// </remarks>
    private protected virtual async Task ActivateAndRunAsync(Func<TComponent, Task> unitTestAsync)
    {
        IServiceScopeFactory scopeFactory = ServiceProvider.GetRequiredService<IServiceScopeFactory>();
        await using AsyncServiceScope scope = scopeFactory.CreateAsyncScope();
        TComponent component = scope.ServiceProvider.Activate<TComponent>();
        await unitTestAsync.Invoke(component);
    }
}
