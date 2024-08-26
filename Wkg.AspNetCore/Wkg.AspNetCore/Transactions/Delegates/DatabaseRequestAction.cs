using Microsoft.EntityFrameworkCore;
using Wkg.AspNetCore.Transactions.Continuations;

namespace Wkg.AspNetCore.Transactions.Actions;

/// <summary>
/// A delegate that represents an isolated API request action that can be executed in a transactional context.
/// </summary>
/// <typeparam name="TDbContext">The type of the database context.</typeparam>
/// <typeparam name="TResult">The type of the result.</typeparam>
/// <param name="dbContext">The database context to be used to interact with the database.</param>
/// <param name="transaction">The transaction to be used to interact with the database.</param>
/// <returns>The <see cref="IDeferredTransactionState{TResult}"/> representing the result of the request action.</returns>
public delegate IDeferredTransactionState<TResult> DatabaseRequestAction<TDbContext, TResult>(TDbContext dbContext, IScopedTransaction transaction) where TDbContext : DbContext;

/// <summary>
/// A delegate that represents an isolated API request action that can be executed in a transactional context.
/// </summary>
/// <typeparam name="TDbContext">The type of the database context.</typeparam>
/// <param name="dbContext">The database context to be used to interact with the database.</param>
/// <param name="transaction">The transaction to be used to interact with the database.</param>
/// <returns>The <see cref="IDeferredTransactionState"/> representing the result of the request action.</returns>
public delegate IDeferredTransactionState DatabaseRequestAction<TDbContext>(TDbContext dbContext, IScopedTransaction transaction) where TDbContext : DbContext;

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