using Microsoft.EntityFrameworkCore;
using Wkg.AspNetCore.Transactions;

namespace Wkg.AspNetCore.Abstractions.Managers;

/// <summary>
/// Provides a base class for API managers that require database access.
/// </summary>
/// <typeparam name="TDbContext">The type of the database context.</typeparam>
/// <remarks>
/// Initializes a new instance of the <see cref="DatabaseManager{TDbContext}"/> class.
/// </remarks>
/// <param name="transactionService">The DI descriptor of the transaction service.</param>
public abstract class DatabaseManager<TDbContext>(ITransactionServiceHandle transactionService) : ManagerBase where TDbContext : DbContext
{

    /// <summary>
    /// Gets the transaction service responsible for managing the database context associated with this context.
    /// </summary>
    protected ITransactionService<TDbContext> Transaction { get; } = transactionService.GetInstance<TDbContext>();
}
