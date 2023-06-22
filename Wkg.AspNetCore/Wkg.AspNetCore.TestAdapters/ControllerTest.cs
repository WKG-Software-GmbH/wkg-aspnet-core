using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Wkg.AspNetCore.Interop;
using Wkg.AspNetCore.TestAdapters.Extensions;

namespace Wkg.AspNetCore.TestAdapters;

public abstract class ControllerTest<TController, TDatabaseLoader> : TestBase 
    where TController : ControllerBase
    where TDatabaseLoader : IDatabaseLoader
{
    private protected override void OnInitialized()
    {
        base.OnInitialized();
        TDatabaseLoader.InitializeDatabase(ServiceProvider);
    }

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
