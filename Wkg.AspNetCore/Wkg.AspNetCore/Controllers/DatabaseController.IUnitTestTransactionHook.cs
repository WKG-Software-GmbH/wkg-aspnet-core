using Wkg.AspNetCore.Interop;

namespace Wkg.AspNetCore.Controllers;

public abstract partial class DatabaseController<TDbContext> : IUnitTestTransactionHook
{
    void IUnitTestTransactionHook.RollbackTransaction__UNIT_TEST_HOOK()
    {
        if (!TransactionManagementAllowed && _isIsolated && DbContext.Database.CurrentTransaction is not null)
        {
            DbContext.Database.CurrentTransaction.Rollback();
            _isIsolated = false;
        }
    }

    async ValueTask IUnitTestTransactionHook.RollbackTransactionAsync__UNIT_TEST_HOOK()
    {
        if (!TransactionManagementAllowed && _isIsolated && DbContext.Database.CurrentTransaction is not null)
        {
            await DbContext.Database.CurrentTransaction.RollbackAsync();
            _isIsolated = false;
        }
    }

    bool IUnitTestTransactionHook.ExternalTransactionManagement__UNIT_TEST_HOOK
    {
        get => TransactionManagementAllowed is false;
        set => TransactionManagementAllowed = value is false;
    }
}
