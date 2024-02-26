using Microsoft.AspNetCore.Mvc;
using Wkg.AspNetCore.Abstractions.Controllers;
using Wkg.AspNetCore.Abstractions.Managers;
using Wkg.AspNetCore.Interop;

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
    /// <inheritdoc cref="TransactionAwareTest{TComponent, TDatabaseLoader}.RunTestUsingTransactionHook(Action{TComponent})"/>/>
    protected virtual void UsingManager(Action<TManager> unitTest) => RunTestUsingTransactionHook(unitTest);

    /// <inheritdoc cref="TransactionAwareTest{TComponent, TDatabaseLoader}.RunTestUsingTransactionHookAsync(Func{TComponent, Task})"/>/>
    protected virtual Task UsingManagerAsync(Func<TManager, Task> unitTestAsync) => RunTestUsingTransactionHookAsync(unitTestAsync);
}
