using Wkg.AspNetCore.TransactionManagement;

namespace Wkg.AspNetCore.Controllers;

public abstract partial class DatabaseController<TDbContext>
{
    /// <summary>
    /// Creates a new <see cref="ITransactionalContinuation"/> that will commit the current transaction.
    /// </summary>
    protected static ITransactionalContinuation Commit() =>
        new TransactionalContinuation(TransactionalContinuationType.Commit);

    /// <summary>
    /// Creates a new <see cref="ITransactionalContinuation"/> that will rollback the current transaction.
    /// </summary>
    protected static ITransactionalContinuation Rollback() =>
        new TransactionalContinuation(TransactionalContinuationType.Rollback);

    /// <summary>
    /// Creates a new <see cref="ITransactionalContinuation{TResult}"/> that will commit the current transaction and return the specified <paramref name="result"/>.
    /// </summary>
    protected static ITransactionalContinuation<TResult> Commit<TResult>(TResult result) =>
        new TransactionalContinuation<TResult>(TransactionalContinuationType.Commit, result);

    /// <summary>
    /// Creates a new <see cref="ITransactionalContinuation{TResult}"/> that will rollback the current transaction and return the specified <paramref name="result"/>.
    /// </summary>
    protected static ITransactionalContinuation<TResult> Rollback<TResult>(TResult result) =>
        new TransactionalContinuation<TResult>(TransactionalContinuationType.Rollback, result);
}
