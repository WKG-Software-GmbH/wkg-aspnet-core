namespace Wkg.AspNetCore.Transactions.Continuations;

internal readonly record struct DeferredTransactionState
(
    TransactionState NextState
) : IDeferredTransactionState;
