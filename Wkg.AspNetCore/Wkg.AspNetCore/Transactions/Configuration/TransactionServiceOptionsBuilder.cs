using System.Data;

namespace Wkg.AspNetCore.Transactions.Configuration;

/// <summary>
/// Provides a fluent API for configuring the default options for <see cref="ITransactionService{TDbContext}"/> instances.
/// </summary>
public class TransactionServiceOptionsBuilder
{
    internal IsolationLevel TransactionIsolationLevel { get; private set; } = IsolationLevel.ReadCommitted;

    /// <summary>
    /// Sets the default <see cref="IsolationLevel"/> to be used by <see cref="ITransactionService{TDbContext}"/> instances for all database transactions.
    /// </summary>
    /// <param name="isolationLevel">The <see cref="IsolationLevel"/> to be used.</param>
    /// <returns>The same <see cref="TransactionServiceOptionsBuilder"/> instance for fluent configuration.</returns>
    /// <remarks>
    /// Isolation levels only apply when <see cref="ITransactionService{TDbContext}"/> instances are used to perform database transactions.
    /// </remarks>
    public TransactionServiceOptionsBuilder UseIsolationLevel(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
    {
        TransactionIsolationLevel = isolationLevel;
        return this;
    }
}