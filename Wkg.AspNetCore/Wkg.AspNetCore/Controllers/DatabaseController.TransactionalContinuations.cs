using Wkg.AspNetCore.TransactionManagement;

namespace Wkg.AspNetCore.Controllers;

public abstract partial class DatabaseController<TDbContext>
{
    protected static ITransactionalContinuation Commit() =>
        new TransactionalContinuation(TransactionalContinuationType.Commit);

    protected static ITransactionalContinuation Rollback() =>
        new TransactionalContinuation(TransactionalContinuationType.Rollback);

    protected static ITransactionalContinuation<TResult> Commit<TResult>(TResult result) =>
        new TransactionalContinuation<TResult>(TransactionalContinuationType.Commit, result);

    protected static ITransactionalContinuation<TResult> Rollback<TResult>(TResult result) =>
        new TransactionalContinuation<TResult>(TransactionalContinuationType.Rollback, result);
}
