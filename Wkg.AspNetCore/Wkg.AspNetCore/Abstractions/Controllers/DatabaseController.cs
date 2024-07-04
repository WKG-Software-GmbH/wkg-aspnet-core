using Microsoft.EntityFrameworkCore;
using Wkg.AspNetCore.Transactions;

namespace Wkg.AspNetCore.Abstractions.Controllers;

/// <summary>
/// Provides a base class for API controllers that require database access.
/// </summary>
/// <typeparam name="TDbContext">The type of the database context.</typeparam>
public abstract class DatabaseController<TDbContext>(ITransactionManager transactionManager) : WkgControllerBase(transactionManager.ErrorHandler) where TDbContext : DbContext
{
    /// <summary>
    /// Gets the transaction manager for the database context.
    /// </summary>
    protected ITransactionManager<TDbContext> Transaction { get; } = transactionManager.GetInstance<TDbContext>();
}
