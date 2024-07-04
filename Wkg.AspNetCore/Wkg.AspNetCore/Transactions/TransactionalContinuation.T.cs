namespace Wkg.AspNetCore.TransactionManagement;

internal readonly record struct TransactionalContinuation<TResult>
(
    TransactionalContinuationType NextAction, 
    TResult Result
) : ITransactionalContinuation<TResult>;