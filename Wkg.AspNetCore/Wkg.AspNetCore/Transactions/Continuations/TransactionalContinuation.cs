namespace Wkg.AspNetCore.Transactions.Continuations;

internal readonly record struct TransactionalContinuation
(
    TransactionState NextAction
) : ITransactionalContinuation;
