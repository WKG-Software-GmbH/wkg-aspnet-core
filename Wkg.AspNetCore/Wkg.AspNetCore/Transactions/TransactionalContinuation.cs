namespace Wkg.AspNetCore.TransactionManagement;

internal readonly record struct TransactionalContinuation
(
    TransactionalContinuationType NextAction
) : ITransactionalContinuation;
