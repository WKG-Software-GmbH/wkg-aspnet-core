using Wkg.AspNetCore.Interop;
using Wkg.AspNetCore.TransactionManagement;

namespace Wkg.AspNetCore.Controllers;

public abstract partial class DatabaseController<TDbContext> : IUnitTestTransactionHook
{
    void IUnitTestTransactionHook.RollbackTransaction__UNIT_TEST_HOOK()
    {
        if (!TransactionManagementAllowed && _isIsolated && DbContext.Database.CurrentTransaction is not null)
        {
            DbContext.Database.CurrentTransaction.Rollback();
            _isIsolated = false;

            // reset the continuation type to Commit, which is the expected default
            // even if we don't want to commit the transaction.
            // nothing will be committed anyway because transaction management is disabled (not allowed)
            _continuationType = TransactionalContinuationType.Commit;
        }
    }

    async ValueTask IUnitTestTransactionHook.RollbackTransactionAsync__UNIT_TEST_HOOK()
    {
        if (!TransactionManagementAllowed && _isIsolated && DbContext.Database.CurrentTransaction is not null)
        {
            await DbContext.Database.CurrentTransaction.RollbackAsync();
            _isIsolated = false;

            // reset the continuation type to Commit, which is the expected default
            // even if we don't want to commit the transaction.
            // nothing will be committed anyway because transaction management is disabled (not allowed)
            _continuationType = TransactionalContinuationType.Commit;
        }
    }

    bool IUnitTestTransactionHook.ExternalTransactionManagement__UNIT_TEST_HOOK
    {
        get => TransactionManagementAllowed is false;
        set => TransactionManagementAllowed = value is false;
    }
}
