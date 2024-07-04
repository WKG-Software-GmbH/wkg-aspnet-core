namespace Wkg.AspNetCore.Transactions.Continuations;

internal readonly record struct TransactionalContinuation
(
    TransactionResult NextAction
) : ITransactionalContinuation;
