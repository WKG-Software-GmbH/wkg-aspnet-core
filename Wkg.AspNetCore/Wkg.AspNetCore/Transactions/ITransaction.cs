﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Data;
using Wkg.AspNetCore.Exceptions;
using Wkg.AspNetCore.Transactions.Continuations;
using Wkg.AspNetCore.Transactions.Delegates;

namespace Wkg.AspNetCore.Transactions;

/// <summary>
/// Represents a transaction that can be used to execute database requests in an isolated environment.
/// A transaction may flow through multiple layers of the application and is used to ensure that all database interactions within its scope are executed in a single database transaction.
/// The transaction is automatically committed or rolled back when the instance is disposed.
/// </summary>
/// <typeparam name="TDbContext">The type of the database context to be used for the transaction.</typeparam>
public interface ITransaction<TDbContext> : IAsyncDisposable where TDbContext : DbContext
{
    /// <summary>
    /// Represents the state of the transaction.
    /// This state defines to the action that will be taken when the transaction is disposed.
    /// </summary>
    TransactionState State { get; }

    /// <summary>
    /// Retrieves the <see cref="System.Data.IsolationLevel"/> of this transaction.
    /// </summary>
    IsolationLevel IsolationLevel { get; internal set; }

    internal AsyncServiceScope? SlavedScope { get; set; }

    /// <summary>
    /// Executes the specified <paramref name="action"/> in an isolated readonly database transaction with automatic error handling.
    /// </summary>
    /// <remarks>
    /// As the specified action does not require write-access to the database,
    /// the transaction may automatically be rolled back after the action has been executed,
    /// if no other actions requiring write-access are executed within the same transaction scope.
    /// </remarks>
    /// <param name="action">The action to be executed in the isolated environment.</param>
    /// <exception cref="ApiProxyException">if the <paramref name="action"/> throws an exception.</exception>
    /// <exception cref="ConcurrencyViolationException">if <paramref name="action"/> is a <see cref="Task"/>-returning method.</exception>
    IActionResult RunReadOnly(ReadOnlyDatabaseRequestAction<TDbContext, IActionResult> action);

    /// <summary>
    /// Executes the specified <paramref name="action"/> in an isolated database transaction with automatic error handling.
    /// </summary>
    /// <param name="action">The action to be executed in the isolated environment.</param>
    /// <exception cref="ApiProxyException">if the <paramref name="action"/> throws an exception.</exception>
    /// <exception cref="ConcurrencyViolationException">if <paramref name="action"/> is a <see cref="Task"/>-returning method.</exception>
    IActionResult Run(DatabaseRequestAction<TDbContext, IActionResult> action);

    /// <summary>
    /// Executes the specified <paramref name="action"/> in an isolated readonly database transaction with automatic error handling.
    /// </summary>
    /// <remarks>
    /// As the specified action does not require write-access to the database,
    /// the transaction may automatically be rolled back after the action has been executed,
    /// if no other actions requiring write-access are executed within the same transaction scope.
    /// </remarks>
    /// <param name="action">The action to be executed in the isolated environment.</param>
    /// <exception cref="ApiProxyException">if the <paramref name="action"/> throws an exception.</exception>
    /// <exception cref="ConcurrencyViolationException">if <paramref name="action"/> is a <see cref="Task"/>-returning method.</exception>
    void RunReadOnly(ReadOnlyDatabaseRequestAction<TDbContext> action);

    /// <summary>
    /// Executes the specified <paramref name="action"/> in an isolated database transaction with automatic error handling.
    /// </summary>
    /// <param name="action">The action to be executed in the isolated environment.</param>
    /// <exception cref="ApiProxyException">if the <paramref name="action"/> throws an exception.</exception>
    /// <exception cref="ConcurrencyViolationException">if <paramref name="action"/> is a <see cref="Task"/>-returning method.</exception>
    void Run(DatabaseRequestAction<TDbContext> action);

    /// <summary>
    /// Executes the specified <paramref name="action"/> in an isolated readonly database transaction with automatic error handling.
    /// </summary>
    /// <remarks>
    /// As the specified action does not require write-access to the database,
    /// the transaction may automatically be rolled back after the action has been executed,
    /// if no other actions requiring write-access are executed within the same transaction scope.
    /// </remarks>
    /// <typeparam name="TResult">The result of the <paramref name="action"/>.</typeparam>
    /// <param name="action">The action to be executed in the isolated environment.</param>
    /// <returns>The result of the <paramref name="action"/>.</returns>
    /// <exception cref="ApiProxyException">if the <paramref name="action"/> throws an exception.</exception>
    /// <exception cref="ConcurrencyViolationException">if <paramref name="action"/> is a <see cref="Task"/>-returning method.</exception>
    TResult RunReadOnly<TResult>(ReadOnlyDatabaseRequestAction<TDbContext, TResult> action);

    /// <summary>
    /// Executes the specified <paramref name="action"/> in an isolated database transaction with automatic error handling.
    /// </summary>
    /// <typeparam name="TResult">The result of the <paramref name="action"/>.</typeparam>
    /// <param name="action">The action to be executed in the isolated environment.</param>
    /// <returns>The result of the <paramref name="action"/>.</returns>
    /// <exception cref="ApiProxyException">if the <paramref name="action"/> throws an exception.</exception>
    /// <exception cref="ConcurrencyViolationException">if <paramref name="action"/> is a <see cref="Task"/>-returning method.</exception>
    TResult Run<TResult>(DatabaseRequestAction<TDbContext, TResult> action);

    /// <summary>
    /// Executes the specified asynchronous <paramref name="task"/> in an isolated readonly database transaction with automatic error handling.
    /// </summary>
    /// <remarks>
    /// As the specified action does not require write-access to the database,
    /// the transaction may automatically be rolled back after the action has been executed,
    /// if no other actions requiring write-access are executed within the same transaction scope.
    /// </remarks>
    /// <param name="task">The action to be executed in the isolated environment.</param>
    /// <returns>The asynchronous result of the <paramref name="task"/>.</returns>
    /// <exception cref="ApiProxyException">if the <paramref name="task"/> throws an exception.</exception>
    Task<IActionResult> RunReadOnlyAsync(ReadOnlyDatabaseRequestTask<TDbContext, IActionResult> task);

    /// <summary>
    /// Executes the specified asynchronous <paramref name="task"/> in an isolated database transaction with automatic error handling.
    /// </summary>
    /// <param name="task">The action to be executed in the isolated environment.</param>
    /// <returns>The asynchronous result of the <paramref name="task"/>.</returns>
    /// <exception cref="ApiProxyException">if the <paramref name="task"/> throws an exception.</exception>
    Task<IActionResult> RunAsync(DatabaseRequestTask<TDbContext, IActionResult> task);

    /// <summary>
    /// Executes the specified asynchronous <paramref name="task"/> in an isolated readonly database transaction with automatic error handling.
    /// </summary>
    /// <remarks>
    /// As the specified action does not require write-access to the database,
    /// the transaction may automatically be rolled back after the action has been executed,
    /// if no other actions requiring write-access are executed within the same transaction scope.
    /// </remarks>
    /// <param name="task">The action to be executed in the isolated environment.</param>
    /// <returns>The asynchronous result of the <paramref name="task"/>.</returns>
    /// <exception cref="ApiProxyException">if the <paramref name="task"/> throws an exception.</exception>
    Task RunReadOnlyAsync(ReadOnlyDatabaseRequestTask<TDbContext> task);

    /// <summary>
    /// Executes the specified asynchronous <paramref name="task"/> in an isolated database transaction with automatic error handling.
    /// </summary>
    /// <param name="task">The action to be executed in the isolated environment.</param>
    /// <returns>The asynchronous result of the <paramref name="task"/>.</returns>
    /// <exception cref="ApiProxyException">if the <paramref name="task"/> throws an exception.</exception>
    Task RunAsync(DatabaseRequestTask<TDbContext> task);

    /// <summary>
    /// Executes the specified asynchronous <paramref name="task"/> in an isolated readonly database transaction with automatic error handling.
    /// </summary>
    /// <remarks>
    /// As the specified action does not require write-access to the database,
    /// the transaction may automatically be rolled back after the action has been executed,
    /// if no other actions requiring write-access are executed within the same transaction scope.
    /// </remarks>
    /// <typeparam name="TResult">The result of the <paramref name="task"/>.</typeparam>
    /// <param name="task">The asynchronous Task to be executed in the isolated environment.</param>
    /// <exception cref="ApiProxyException">if the <paramref name="task"/> throws an exception.</exception>
    Task<TResult> RunReadOnlyAsync<TResult>(ReadOnlyDatabaseRequestTask<TDbContext, TResult> task);

    /// <summary>
    /// Executes the specified asynchronous <paramref name="task"/> in an isolated database transaction with automatic error handling.
    /// </summary>
    /// <typeparam name="TResult">The result of the <paramref name="task"/>.</typeparam>
    /// <param name="task">The asynchronous Task to be executed in the isolated environment.</param>
    /// <exception cref="ApiProxyException">if the <paramref name="task"/> throws an exception.</exception>
    Task<TResult> RunAsync<TResult>(DatabaseRequestTask<TDbContext, TResult> task);
}