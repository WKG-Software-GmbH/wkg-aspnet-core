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
/// <param name="transactionService">The DI service that provides transaction management.</param>
public abstract class DatabaseService<TDbContext>(ITransactionServiceHandle transactionService) : WkgServiceBase(transactionService.ErrorSentry) where TDbContext : DbContext
{
    /// <summary>
    /// Gets the transaction service responsible for managing the database context associated with this context.
    /// </summary>
    protected ITransactionService<TDbContext> Transaction { get; } = transactionService.GetInstance<TDbContext>();
}