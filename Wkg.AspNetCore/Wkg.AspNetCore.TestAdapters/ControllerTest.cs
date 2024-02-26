using Microsoft.AspNetCore.Mvc;

namespace Wkg.AspNetCore.TestAdapters;

/// <summary>
/// Provides a base class for unit tests targeting an ASP.NET Core controller.
/// </summary>
/// <typeparam name="TController">The type of the controller to test.</typeparam>
/// <typeparam name="TDatabaseLoader">The type of the <see cref="IDatabaseLoader"/> to be used to initialize the database, if necessary.</typeparam>
public abstract class ControllerTest<TController, TDatabaseLoader> : TransactionAwareTest<TController, TDatabaseLoader> 
    where TController : ControllerBase
    where TDatabaseLoader : IDatabaseLoader
{
    /// <inheritdoc cref="TransactionAwareTest{TComponent, TDatabaseLoader}.RunTestUsingTransactionHook(Action{TComponent})"/>/>
    protected virtual void UsingController(Action<TController> unitTest) => RunTestUsingTransactionHook(unitTest);

    /// <inheritdoc cref="TransactionAwareTest{TComponent, TDatabaseLoader}.RunTestUsingTransactionHookAsync(Func{TComponent, Task})"/>/>
    protected virtual Task UsingControllerAsync(Func<TController, Task> unitTestAsync) => RunTestUsingTransactionHookAsync(unitTestAsync);
}
