using System.Data;

namespace Wkg.AspNetCore.Transactions.Configuration;

/// <summary>
/// Provides a fluent API for configuring the default options for <see cref="ITransactionManager{TDbContext}"/> instances.
/// </summary>
public class TransactionManagerOptionsBuilder
{
    internal IsolationLevel TransactionIsolationLevel { get; private set; } = IsolationLevel.ReadCommitted;

    /// <summary>
    /// Sets the default <see cref="IsolationLevel"/> to be used by <see cref="ITransactionManager{TDbContext}"/> instances for all database transactions.
    /// </summary>
    /// <param name="isolationLevel">The <see cref="IsolationLevel"/> to be used.</param>
    /// <returns>The same <see cref="TransactionManagerOptionsBuilder"/> instance for fluent configuration.</returns>
    /// <remarks>
    /// Isolation levels only apply when <see cref="ITransactionManager{TDbContext}"/> instances are used to perform database transactions.
    /// </remarks>
    public TransactionManagerOptionsBuilder UseIsolationLevel(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
    {
        TransactionIsolationLevel = isolationLevel;
        return this;
    }
}