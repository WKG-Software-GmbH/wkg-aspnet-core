using Microsoft.EntityFrameworkCore;
using Wkg.AspNetCore.Transactions;

namespace Wkg.AspNetCore.Abstractions.RazorPages;

/// <summary>
/// Provides a base class for Razor Pages that require database access.
/// </summary>
/// <typeparam name="TDbContext">The type of the database context.</typeparam>
public abstract class DatabasePageModel<TDbContext>(ITransactionManager transactionManager) : WkgPageModel(transactionManager.ErrorSentry) where TDbContext : DbContext
{
    /// <summary>
    /// Gets the transaction manager for the database context.
    /// </summary>
    protected ITransactionManager<TDbContext> Transaction { get; } = transactionManager.GetInstance<TDbContext>();
}
