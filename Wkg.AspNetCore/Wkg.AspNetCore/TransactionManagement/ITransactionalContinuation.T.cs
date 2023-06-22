namespace Wkg.AspNetCore.TransactionManagement;

/// <summary>
/// Represents a continuation of a transactional operation and defines what action should be taken after the operation has completed.
/// </summary>
/// <typeparam name="TResult">The type of the result of the transactional operation.</typeparam>
public interface ITransactionalContinuation<TResult> : ITransactionalContinuation
{
    internal TResult Result { get; }
}
