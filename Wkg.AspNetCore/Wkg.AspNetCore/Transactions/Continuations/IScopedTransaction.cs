namespace Wkg.AspNetCore.Transactions.Continuations;

/// <summary>
/// Represents a scoped transaction and defines what action should be taken after the transaction has completed. 
/// Note that precedence rules apply to the actions defined by the continuations.
/// </summary>
public interface IScopedTransaction
{
    /// <summary>
    /// Creates a new <see cref="ITransactionalContinuation"/> that will commit the current transaction.
    /// </summary>
    ITransactionalContinuation Commit();

    /// <summary>
    /// Creates a new <see cref="ITransactionalContinuation"/> that will rollback the current transaction.
    /// </summary>
    ITransactionalContinuation Rollback();

    /// <summary>
    /// Creates a new <see cref="ITransactionalContinuation{TResult}"/> that will commit the current transaction and return the specified <paramref name="result"/>.
    /// </summary>
    ITransactionalContinuation<TResult> Commit<TResult>(TResult result);

    /// <summary>
    /// Creates a new <see cref="ITransactionalContinuation{TResult}"/> that will rollback the current transaction and return the specified <paramref name="result"/>.
    /// </summary>
    ITransactionalContinuation<TResult> Rollback<TResult>(TResult result);
}