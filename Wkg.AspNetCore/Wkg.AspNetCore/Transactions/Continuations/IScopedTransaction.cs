namespace Wkg.AspNetCore.Transactions.Continuations;

/// <summary>
/// Represents a scoped transaction and defines what action should be taken after the transaction has completed. 
/// Note that precedence rules apply to the actions defined by the continuations.
/// </summary>
public interface IScopedTransaction
{
    /// <summary>
    /// Creates a new <see cref="IDeferredTransactionState"/> that will commit the current transaction.
    /// </summary>
    IDeferredTransactionState Commit();

    /// <summary>
    /// Creates a new <see cref="IDeferredTransactionState"/> that will rollback the current transaction.
    /// </summary>
    IDeferredTransactionState Rollback();

    /// <summary>
    /// Creates a new <see cref="IDeferredTransactionState{TResult}"/> that will commit the current transaction and return the specified <paramref name="result"/>.
    /// </summary>
    IDeferredTransactionState<TResult> Commit<TResult>(TResult result);

    /// <summary>
    /// Creates a new <see cref="IDeferredTransactionState{TResult}"/> that will rollback the current transaction and return the specified <paramref name="result"/>.
    /// </summary>
    IDeferredTransactionState<TResult> Rollback<TResult>(TResult result);
}