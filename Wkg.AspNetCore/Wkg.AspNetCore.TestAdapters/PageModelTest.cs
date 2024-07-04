using Microsoft.AspNetCore.Mvc;

namespace Wkg.AspNetCore.TestAdapters;

/// <summary>
/// Provides a base class for unit tests targeting a Razor Pages model.
/// </summary>
/// <typeparam name="TPageModel">The type of the controller to test.</typeparam>
/// <typeparam name="TDatabaseLoader">The type of the <see cref="IDatabaseLoader"/> to be used to initialize the database, if necessary.</typeparam>
public abstract class PageModelTest<TPageModel, TDatabaseLoader> : TransactionAwareTest<TPageModel, TDatabaseLoader> 
    where TPageModel : ControllerBase
    where TDatabaseLoader : IDatabaseLoader
{
    /// <inheritdoc cref="TransactionAwareTest{TComponent, TDatabaseLoader}.ActivateAndRun(Action{TComponent})"/>/>
    protected virtual void UsingPageModel(Action<TPageModel> unitTest) => ActivateAndRun(unitTest);

    /// <inheritdoc cref="TransactionAwareTest{TComponent, TDatabaseLoader}.ActivateAndRunAsync(Func{TComponent, Task})"/>/>
    protected virtual Task UsingPageModelAsync(Func<TPageModel, Task> unitTestAsync) => ActivateAndRunAsync(unitTestAsync);
}
