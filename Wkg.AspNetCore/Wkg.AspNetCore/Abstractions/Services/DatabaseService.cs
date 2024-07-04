using Microsoft.EntityFrameworkCore;
using Wkg.AspNetCore.Transactions;

namespace Wkg.AspNetCore.Abstractions.Services;

/// <summary>
/// Represents an independent scoped service that requires database access.
/// </summary>
/// <typeparam name="TDbContext">The type of the database context.</typeparam>
/// <remarks>
/// Initializes a new instance of the <see cref="DatabaseService{TDbContext}"/> class.
/// </remarks>
/// <param name="transactionManager">The DI service that provides transaction management.</param>
public abstract class DatabaseService<TDbContext>(ITransactionManager transactionManager) : WkgServiceBase(transactionManager.ErrorHandler) where TDbContext : DbContext
{
    protected ITransactionManager<TDbContext> Transaction { get; } = transactionManager.GetInstance<TDbContext>();
}