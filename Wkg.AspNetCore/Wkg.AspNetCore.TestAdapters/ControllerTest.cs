using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Wkg.AspNetCore.Interop;
using Wkg.AspNetCore.TestAdapters.Extensions;

namespace Wkg.AspNetCore.TestAdapters;

/// <summary>
/// Provides a base class for unit tests targeting an ASP.NET Core controller.
/// </summary>
/// <typeparam name="TController">The type of the controller to test.</typeparam>
/// <typeparam name="TDatabaseLoader">The type of the <see cref="IDatabaseLoader"/> to be used to initialize the database, if necessary.</typeparam>
public abstract class ControllerTest<TController, TDatabaseLoader> : TestBase 
    where TController : ControllerBase
    where TDatabaseLoader : IDatabaseLoader
{
    private protected override void OnInitialized()
    {
        base.OnInitialized();
        TDatabaseLoader.InitializeDatabase(ServiceProvider);
    }

    /// <summary>
    /// Executes the specified unit test against the controller.
    /// </summary>
    /// <param name="unitTest">The unit test to be executed.</param>
    /// <remarks>
    /// Any transactional context created by the controller will be rolled back after the unit test has been executed.
    /// </remarks>
    protected virtual void UsingController(Action<TController> unitTest)
    {
        TController controller = ServiceProvider.ActivateController<TController>();
        IUnitTestTransactionHook? transactionHook = controller as IUnitTestTransactionHook;
        if (transactionHook is not null)
        {
            transactionHook.ExternalTransactionManagement__UNIT_TEST_HOOK = true;
        }
        try
        {
            unitTest.Invoke(controller);
        }
        finally
        {
            transactionHook?.RollbackTransaction__UNIT_TEST_HOOK();
        }
    }

    /// <summary>
    /// Executes the specified unit test asynchronously against the controller.
    /// </summary>
    /// <param name="unitTestAsync">The unit test to be executed asynchronously.</param>
    /// <remarks>
    /// Any transactional context created by the controller will be rolled back after the unit test has been executed.
    /// </remarks>
    protected static async Task UsingControllerAsync(Func<TController, Task> unitTestAsync)
    {
        TController controller = ServiceProvider.ActivateController<TController>();
        IUnitTestTransactionHook? transactionHook = controller as IUnitTestTransactionHook;
        if (transactionHook is not null)
        {
            transactionHook.ExternalTransactionManagement__UNIT_TEST_HOOK = true;
        }
        try
        {
            await unitTestAsync.Invoke(controller);
        }
        finally
        {
            if (transactionHook is not null)
            {
                await transactionHook.RollbackTransactionAsync__UNIT_TEST_HOOK();
            }
        }
    }
}
