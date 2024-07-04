namespace Wkg.AspNetCore.Transactions.Continuations;

internal class ScopedTransaction : IScopedTransaction
{
    public ITransactionalContinuation Commit() =>
        new TransactionalContinuation(TransactionResult.Commit);

    public ITransactionalContinuation Rollback() =>
        new TransactionalContinuation(TransactionResult.Rollback);

    public ITransactionalContinuation<TResult> Commit<TResult>(TResult result) =>
        new TransactionalContinuation<TResult>(TransactionResult.Commit, result);

    public ITransactionalContinuation<TResult> Rollback<TResult>(TResult result) =>
        new TransactionalContinuation<TResult>(TransactionResult.Rollback, result);
}