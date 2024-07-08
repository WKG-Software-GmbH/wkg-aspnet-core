namespace Wkg.AspNetCore.Transactions.Continuations;

internal class ScopedTransaction : IScopedTransaction
{
    public ITransactionalContinuation Commit() =>
        new TransactionalContinuation(TransactionState.Commit);

    public ITransactionalContinuation Rollback() =>
        new TransactionalContinuation(TransactionState.Rollback);

    public ITransactionalContinuation<TResult> Commit<TResult>(TResult result) =>
        new TransactionalContinuation<TResult>(TransactionState.Commit, result);

    public ITransactionalContinuation<TResult> Rollback<TResult>(TResult result) =>
        new TransactionalContinuation<TResult>(TransactionState.Rollback, result);
}