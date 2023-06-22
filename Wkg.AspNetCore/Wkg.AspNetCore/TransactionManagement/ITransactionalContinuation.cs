using System.Diagnostics;

namespace Wkg.AspNetCore.TransactionManagement;

/// <summary>
/// Represents a continuation of a transactional operation and defines what action should be taken after the operation has completed.
/// </summary>
public interface ITransactionalContinuation
{
    internal TransactionalContinuationType NextAction { get; }
}
