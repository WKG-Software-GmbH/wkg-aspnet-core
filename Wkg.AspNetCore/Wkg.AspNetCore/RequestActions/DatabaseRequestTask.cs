using Microsoft.EntityFrameworkCore;
using Wkg.AspNetCore.TransactionManagement;

namespace Wkg.AspNetCore.RequestActions;

/// <summary>
/// A delegate that represents an isolated asynchronous API request action that can be executed in a transactional context.
/// </summary>
/// <typeparam name="TDbContext">The type of the database context.</typeparam>
/// <typeparam name="TResult">The type of the result.</typeparam>
/// <param name="dbContext">The database context to be used to interact with the database.</param>
/// <returns>A <see cref="Task"/> that can be used to retrieve the <see cref="ITransactionalContinuation{TResult}"/> representing the result of the asynchronous request action.</returns>
public delegate Task<ITransactionalContinuation<TResult>> DatabaseRequestTask<TDbContext, TResult>(TDbContext dbContext) where TDbContext : DbContext;

/// <summary>
/// A delegate that represents an isolated asynchronous API request action that can be executed in a transactional context.
/// </summary>
/// <typeparam name="TDbContext">The type of the database context.</typeparam>
/// <param name="dbContext">The database context to be used to interact with the database.</param>
/// <returns>A <see cref="Task"/> that can be used to retrieve the <see cref="ITransactionalContinuation"/> representing the result of the asynchronous request action.</returns>
public delegate Task<ITransactionalContinuation> DatabaseRequestTask<TDbContext>(TDbContext dbContext) where TDbContext : DbContext;