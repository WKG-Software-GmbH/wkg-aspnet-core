namespace Wkg.AspNetCore.TransactionManagement;

public interface ITransactionalContinuation<TResult> : ITransactionalContinuation
{
    internal TResult Result { get; }
}
