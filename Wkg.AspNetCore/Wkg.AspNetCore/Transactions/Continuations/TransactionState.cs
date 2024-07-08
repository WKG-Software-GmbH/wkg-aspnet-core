namespace Wkg.AspNetCore.Transactions.Continuations;

/// <summary>
/// Represents a continuation of a transactional operation and defines what action should be taken after the lifecycle of the transaction scope has completed.
/// </summary>
/// <remarks>
/// Continuations are designed to be OR-combinable using the bitwise OR operator, so higher priority continuations can override lower priority continuations.
/// </remarks>
[Flags]
public enum TransactionState : uint
{
    /// <summary>
    /// Unless otherwise specified, the transaction should be rolled back as no changes are expected to be made, yielding to higher priority continuations as necessary.
    /// As such, read-only operations may result in a commit if a different continuation on the same scope requests it.
    /// </summary>
    ReadOnly = 0,

    /// <summary>
    /// Unless otherwise specified, the transaction should be committed if no higher priority continuations request otherwise.
    /// If a higher priority continuation requests a rollback, the transaction will be rolled back.
    /// </summary>
    Commit = 1,

    /// <summary>
    /// The transaction should be rolled back, overriding any requests to commit the transaction.
    /// </summary>
    Rollback = 3,

    /// <summary>
    /// An unexpected error occurred during the transaction. The transaction will be rolled back, regardless of continuations specified by user code.
    /// </summary>
    ExceptionalRollback = 7,

    /// <summary>
    /// The underlying transaction was executed against the database with the specified final flags and cannot be further modified.
    /// </summary>
    Finalized = 8
}