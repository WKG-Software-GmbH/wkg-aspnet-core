namespace Wkg.AspNetCore.Transactions.Continuations;

internal readonly record struct TransactionalContinuation<TResult>
(
    TransactionState NextAction,
    TResult Result
) : ITransactionalContinuation<TResult>;