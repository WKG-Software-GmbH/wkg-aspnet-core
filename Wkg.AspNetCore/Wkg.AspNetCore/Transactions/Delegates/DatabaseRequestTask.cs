﻿using Microsoft.EntityFrameworkCore;
using Wkg.AspNetCore.Transactions.Continuations;

namespace Wkg.AspNetCore.Transactions.Delegates;

/// <summary>
/// A delegate that represents an isolated asynchronous API request action that can be executed in a transactional context.
/// </summary>
/// <typeparam name="TDbContext">The type of the database context.</typeparam>
/// <typeparam name="TResult">The type of the result.</typeparam>
/// <param name="dbContext">The database context to be used to interact with the database.</param>
/// <param name="transaction">The transaction to be used to interact with the database.</param>
/// <returns>A <see cref="Task"/> that can be used to retrieve the <see cref="IDeferredTransactionState{TResult}"/> representing the result of the asynchronous request action.</returns>
public delegate Task<IDeferredTransactionState<TResult>> DatabaseRequestTask<TDbContext, TResult>(TDbContext dbContext, IScopedTransaction transaction) where TDbContext : DbContext;

/// <summary>
/// A delegate that represents an isolated asynchronous API request action that can be executed in a transactional context.
/// </summary>
/// <typeparam name="TDbContext">The type of the database context.</typeparam>
/// <param name="dbContext">The database context to be used to interact with the database.</param>
/// <param name="transaction">The transaction to be used to interact with the database.</param>
/// <returns>A <see cref="Task"/> that can be used to retrieve the <see cref="IDeferredTransactionState"/> representing the result of the asynchronous request action.</returns>
public delegate Task<IDeferredTransactionState> DatabaseRequestTask<TDbContext>(TDbContext dbContext, IScopedTransaction transaction) where TDbContext : DbContext;

/// <summary>
/// A delegate that represents an isolated asynchronous API request action that can be executed in a readonly transactional context.
/// </summary>
/// <typeparam name="TDbContext">The type of the database context.</typeparam>
/// <typeparam name="TResult">The type of the result.</typeparam>
/// <param name="dbContext">The database context to be used to interact with the database.</param>
/// <returns>A <see cref="Task"/> that can be used to retrieve the result of the asynchronous request action.</returns>
public delegate Task<TResult> ReadOnlyDatabaseRequestTask<TDbContext, TResult>(TDbContext dbContext) where TDbContext : DbContext;

/// <summary>
/// A delegate that represents an isolated asynchronous API request action that can be executed in a readonly transactional context.
/// </summary>
/// <typeparam name="TDbContext">The type of the database context.</typeparam>
/// <param name="dbContext">The database context to be used to interact with the database.</param>
/// <returns>A <see cref="Task"/> representing the result of the asynchronous request action.</returns>
public delegate Task ReadOnlyDatabaseRequestTask<TDbContext>(TDbContext dbContext) where TDbContext : DbContext;