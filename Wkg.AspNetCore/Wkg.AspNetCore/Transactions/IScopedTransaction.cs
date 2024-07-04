using Wkg.AspNetCore.TransactionManagement;

namespace Wkg.AspNetCore.Transactions;

public interface IScopedTransaction
{
    /// <summary>
    /// Creates a new <see cref="ITransactionalContinuation"/> that will commit the current transaction.
    /// </summary>
    ITransactionalContinuation Commit();

    /// <summary>
    /// Creates a new <see cref="ITransactionalContinuation"/> that will rollback the current transaction.
    /// </summary>
    ITransactionalContinuation Rollback();

    /// <summary>
    /// Creates a new <see cref="ITransactionalContinuation{TResult}"/> that will commit the current transaction and return the specified <paramref name="result"/>.
    /// </summary>
    ITransactionalContinuation<TResult> Commit<TResult>(TResult result);

    /// <summary>
    /// Creates a new <see cref="ITransactionalContinuation{TResult}"/> that will rollback the current transaction and return the specified <paramref name="result"/>.
    /// </summary>
    ITransactionalContinuation<TResult> Rollback<TResult>(TResult result);
}

internal class ScopedTransaction : IScopedTransaction
{
    public ITransactionalContinuation Commit() =>
        new TransactionalContinuation(TransactionalContinuationType.Commit);

    public ITransactionalContinuation Rollback() =>
        new TransactionalContinuation(TransactionalContinuationType.Rollback);

    public ITransactionalContinuation<TResult> Commit<TResult>(TResult result) =>
        new TransactionalContinuation<TResult>(TransactionalContinuationType.Commit, result);

    public ITransactionalContinuation<TResult> Rollback<TResult>(TResult result) =>
        new TransactionalContinuation<TResult>(TransactionalContinuationType.Rollback, result);
}