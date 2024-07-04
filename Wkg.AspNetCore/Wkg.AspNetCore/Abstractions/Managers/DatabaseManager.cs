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
/// <param name="transactionManager">The DI descriptor of the transaction manager.</param>
public abstract class DatabaseManager<TDbContext>(ITransactionManager transactionManager) : ManagerBase where TDbContext : DbContext
{
    protected ITransactionManager<TDbContext> Transaction { get; } = transactionManager.GetInstance<TDbContext>();
}
