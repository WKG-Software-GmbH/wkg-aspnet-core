using Microsoft.EntityFrameworkCore;
using Wkg.AspNetCore.Transactions;

namespace Wkg.AspNetCore.Abstractions.Controllers;

/// <summary>
/// Provides a base class for API controllers that require database access.
/// </summary>
/// <typeparam name="TDbContext">The type of the database context.</typeparam>
/// <remarks>
/// Initializes a new instance of the <see cref="DatabaseController{TDbContext}"/> class.
/// </remarks>
/// <param name="transactionService">The DI descriptor of the transaction service.</param>
public abstract class DatabaseController<TDbContext>(ITransactionServiceHandle transactionService) : WkgControllerBase(transactionService.ErrorSentry) where TDbContext : DbContext
{
    /// <summary>
    /// Gets the transaction service responsible for managing the database context associated with this context.
    /// </summary>
    protected ITransactionService<TDbContext> Transaction { get; } = transactionService.GetInstance<TDbContext>();
}
