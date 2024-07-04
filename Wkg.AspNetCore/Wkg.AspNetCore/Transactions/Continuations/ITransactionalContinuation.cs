namespace Wkg.AspNetCore.Transactions.Continuations;

/// <summary>
/// Represents a continuation of a transactional operation and defines what action should be taken after the operation has completed.
/// </summary>
public interface ITransactionalContinuation
{
    internal TransactionResult NextAction { get; }
}
