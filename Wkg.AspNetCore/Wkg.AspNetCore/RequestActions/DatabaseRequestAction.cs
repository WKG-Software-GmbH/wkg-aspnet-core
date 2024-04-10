using Microsoft.EntityFrameworkCore;
using Wkg.AspNetCore.TransactionManagement;

namespace Wkg.AspNetCore.RequestActions;

/// <summary>
/// A delegate that represents an isolated API request action that can be executed in a transactional context.
/// </summary>
/// <typeparam name="TDbContext">The type of the database context.</typeparam>
/// <typeparam name="TResult">The type of the result.</typeparam>
/// <param name="dbContext">The database context to be used to interact with the database.</param>
/// <returns>The <see cref="ITransactionalContinuation{TResult}"/> representing the result of the request action.</returns>
public delegate ITransactionalContinuation<TResult> DatabaseRequestAction<TDbContext, TResult>(TDbContext dbContext) where TDbContext : DbContext;

/// <summary>
/// A delegate that represents an isolated API request action that can be executed in a transactional context.
/// </summary>
/// <typeparam name="TDbContext">The type of the database context.</typeparam>
/// <param name="dbContext">The database context to be used to interact with the database.</param>
/// <returns>The <see cref="ITransactionalContinuation"/> representing the result of the request action.</returns>
public delegate ITransactionalContinuation DatabaseRequestAction<TDbContext>(TDbContext dbContext) where TDbContext : DbContext;

/// <summary>
/// A delegate that represents an isolated API request action that can be executed in a readonly transactional context.
/// </summary>
/// <typeparam name="TDbContext">The type of the database context.</typeparam>
/// <typeparam name="TResult">The type of the result.</typeparam>
/// <param name="dbContext">The database context to be used to interact with the database.</param>
/// <returns>The result of the request action.</returns>
public delegate TResult ReadOnlyDatabaseRequestAction<TDbContext, TResult>(TDbContext dbContext) where TDbContext : DbContext;

/// <summary>
/// A delegate that represents an isolated API request action that can be executed in a readonly transactional context.
/// </summary>
/// <typeparam name="TDbContext">The type of the database context.</typeparam>
/// <param name="dbContext">The database context to be used to interact with the database.</param>
public delegate void ReadOnlyDatabaseRequestAction<TDbContext>(TDbContext dbContext) where TDbContext : DbContext;