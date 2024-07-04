using Wkg.AspNetCore.Abstractions.Managers;

namespace Wkg.AspNetCore.TestAdapters;

/// <summary>
/// Provides a base class for unit tests targeting an ASP.NET Core manager.
/// </summary>
/// <typeparam name="TManager">The type of the controller to test.</typeparam>
/// <typeparam name="TDatabaseLoader">The type of the <see cref="IDatabaseLoader"/> to be used to initialize the database, if necessary.</typeparam>
public abstract class ManagerTest<TManager, TDatabaseLoader> : TransactionAwareTest<TManager, TDatabaseLoader> 
    where TManager : ManagerBase
    where TDatabaseLoader : IDatabaseLoader
{
    /// <inheritdoc cref="TransactionAwareTest{TComponent, TDatabaseLoader}.ActivateAndRun(Action{TComponent})"/>/>
    protected virtual void UsingManager(Action<TManager> unitTest) => ActivateAndRun(unitTest);

    /// <inheritdoc cref="TransactionAwareTest{TComponent, TDatabaseLoader}.ActivateAndRunAsync(Func{TComponent, Task})"/>/>
    protected virtual Task UsingManagerAsync(Func<TManager, Task> unitTestAsync) => ActivateAndRunAsync(unitTestAsync);
}
