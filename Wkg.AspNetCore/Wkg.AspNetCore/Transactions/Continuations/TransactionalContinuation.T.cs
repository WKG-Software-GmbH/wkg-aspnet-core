namespace Wkg.AspNetCore.Transactions.Continuations;

internal readonly record struct TransactionalContinuation<TResult>
(
    TransactionResult NextAction,
    TResult Result
) : ITransactionalContinuation<TResult>;