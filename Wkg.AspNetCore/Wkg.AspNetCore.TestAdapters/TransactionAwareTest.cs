using Wkg.AspNetCore.Interop;
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

    private static IUnitTestTransactionHook? GetTransactionHookOrDefault(TComponent component) => component switch
    {
        IUnitTestTransactionHook hook => hook,
        IUnitTestTransactionHookProxy proxy => proxy.TransactionHookImplementation,
        _ => null
    };

    /// <summary>
    /// Executes the specified unit test against the component in a transactional context.
    /// </summary>
    /// <param name="unitTest">The unit test to be executed.</param>
    /// <remarks>
    /// Any transactional context created by the component will be rolled back after the unit test has been executed.
    /// </remarks>
    private protected virtual void RunTestUsingTransactionHook(Action<TComponent> unitTest)
    {
        TComponent component = ServiceProvider.Activate<TComponent>();
        IUnitTestTransactionHook? transactionHook = GetTransactionHookOrDefault(component);
        if (transactionHook is not null)
        {
            transactionHook.ExternalTransactionManagement__UNIT_TEST_HOOK = true;
        }
        try
        {
            unitTest.Invoke(component);
        }
        finally
        {
            transactionHook?.RollbackTransaction__UNIT_TEST_HOOK();
        }
    }

    /// <summary>
    /// Executes the specified unit test asynchronously against the component in a transactional context.
    /// </summary>
    /// <param name="unitTestAsync">The unit test to be executed asynchronously.</param>
    /// <remarks>
    /// Any transactional context created by the component will be rolled back after the unit test has been executed.
    /// </remarks>
    private protected virtual async Task RunTestUsingTransactionHookAsync(Func<TComponent, Task> unitTestAsync)
    {
        TComponent component = ServiceProvider.Activate<TComponent>();
        IUnitTestTransactionHook? transactionHook = GetTransactionHookOrDefault(component);
        if (transactionHook is not null)
        {
            transactionHook.ExternalTransactionManagement__UNIT_TEST_HOOK = true;
        }
        try
        {
            await unitTestAsync.Invoke(component);
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
