namespace Wkg.AspNetCore.Transactions.Continuations;

internal readonly record struct DeferredTransactionState<TResult>
(
    TransactionState NextState,
    TResult Result
) : IDeferredTransactionState<TResult>;