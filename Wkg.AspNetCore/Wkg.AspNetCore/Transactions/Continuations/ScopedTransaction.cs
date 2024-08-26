namespace Wkg.AspNetCore.Transactions.Continuations;

internal class ScopedTransaction : IScopedTransaction
{
    public IDeferredTransactionState Commit() =>
        new DeferredTransactionState(TransactionState.Commit);

    public IDeferredTransactionState Rollback() =>
        new DeferredTransactionState(TransactionState.Rollback);

    public IDeferredTransactionState<TResult> Commit<TResult>(TResult result) =>
        new DeferredTransactionState<TResult>(TransactionState.Commit, result);

    public IDeferredTransactionState<TResult> Rollback<TResult>(TResult result) =>
        new DeferredTransactionState<TResult>(TransactionState.Rollback, result);
}