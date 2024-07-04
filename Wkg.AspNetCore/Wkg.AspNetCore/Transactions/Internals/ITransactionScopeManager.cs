using Microsoft.EntityFrameworkCore;
using System.Data;

namespace Wkg.AspNetCore.Transactions.Internals;

/// <summary>
/// An internal manager interface for creating transaction scopes.
/// </summary>
/// <typeparam name="TDbContext">The type of the database context.</typeparam>
internal interface ITransactionScopeManager<TDbContext> where TDbContext : DbContext
{
    /// <summary>
    /// Gets or sets the <see cref="IsolationLevel"/> to be used for all transactions of this manager.
    /// </summary>
    internal IsolationLevel TransactionIsolationLevel { get; }

    /// <summary>
    /// Gets the <see cref="IDbContextDescriptor"/> targeting the database context of this manager.
    /// </summary>
    internal IDbContextDescriptor DbContextDescriptor { get; }

    /// <summary>
    /// Creates a new transaction scope for the underlying database context.
    /// </summary>
    public ITransactionScope<TDbContext> CreateScope();
}
