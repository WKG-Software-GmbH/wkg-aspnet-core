using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Wkg.AspNetCore.Exceptions;
using Wkg.AspNetCore.Transactions.Actions;
using Wkg.AspNetCore.Transactions.Continuations;

namespace Wkg.AspNetCore.Transactions;

/// <summary>
/// Represents a transaction scope that can be used to execute database requests in an isolated environment.
/// A transaction scope may flow through multiple layers of the application and is used to ensure that all database interactions within the scope are executed in a single transaction.
/// The transaction is automatically committed or rolled back when the scope is disposed.
/// </summary>
/// <remarks>
/// In ASP.NET Core applications, transaction scopes are typically used to group all database interactions that are executed as part of a single HTTP request.
/// </remarks>
/// <typeparam name="TDbContext"></typeparam>
public interface ITransactionScope<TDbContext> : IAsyncDisposable where TDbContext : DbContext
{
    /// <summary>
    /// Represents the state of the transaction scope.
    /// This state defines to the action that will be taken when the scope is disposed.
    /// </summary>
    TransactionResult TransactionState { get; }

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
    IActionResult ExecuteReadOnly(ReadOnlyDatabaseRequestAction<TDbContext, IActionResult> action);

    /// <summary>
    /// Executes the specified <paramref name="action"/> in an isolated database transaction with automatic error handling.
    /// </summary>
    /// <param name="action">The action to be executed in the isolated environment.</param>
    /// <exception cref="ApiProxyException">if the <paramref name="action"/> throws an exception.</exception>
    /// <exception cref="ConcurrencyViolationException">if <paramref name="action"/> is a <see cref="Task"/>-returning method.</exception>
    IActionResult Execute(DatabaseRequestAction<TDbContext, IActionResult> action);

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
    void ExecuteReadOnly(ReadOnlyDatabaseRequestAction<TDbContext> action);

    /// <summary>
    /// Executes the specified <paramref name="action"/> in an isolated database transaction with automatic error handling.
    /// </summary>
    /// <param name="action">The action to be executed in the isolated environment.</param>
    /// <exception cref="ApiProxyException">if the <paramref name="action"/> throws an exception.</exception>
    /// <exception cref="ConcurrencyViolationException">if <paramref name="action"/> is a <see cref="Task"/>-returning method.</exception>
    void Execute(DatabaseRequestAction<TDbContext> action);

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
    TResult ExecuteReadOnly<TResult>(ReadOnlyDatabaseRequestAction<TDbContext, TResult> action);

    /// <summary>
    /// Executes the specified <paramref name="action"/> in an isolated database transaction with automatic error handling.
    /// </summary>
    /// <typeparam name="TResult">The result of the <paramref name="action"/>.</typeparam>
    /// <param name="action">The action to be executed in the isolated environment.</param>
    /// <returns>The result of the <paramref name="action"/>.</returns>
    /// <exception cref="ApiProxyException">if the <paramref name="action"/> throws an exception.</exception>
    /// <exception cref="ConcurrencyViolationException">if <paramref name="action"/> is a <see cref="Task"/>-returning method.</exception>
    TResult Execute<TResult>(DatabaseRequestAction<TDbContext, TResult> action);

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
    Task<IActionResult> ExecuteReadOnlyAsync(ReadOnlyDatabaseRequestTask<TDbContext, IActionResult> task);

    /// <summary>
    /// Executes the specified asynchronous <paramref name="task"/> in an isolated database transaction with automatic error handling.
    /// </summary>
    /// <param name="task">The action to be executed in the isolated environment.</param>
    /// <returns>The asynchronous result of the <paramref name="task"/>.</returns>
    /// <exception cref="ApiProxyException">if the <paramref name="task"/> throws an exception.</exception>
    Task<IActionResult> ExecuteAsync(DatabaseRequestTask<TDbContext, IActionResult> task);

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
    Task ExecuteReadOnlyAsync(ReadOnlyDatabaseRequestTask<TDbContext> task);

    /// <summary>
    /// Executes the specified asynchronous <paramref name="task"/> in an isolated database transaction with automatic error handling.
    /// </summary>
    /// <param name="task">The action to be executed in the isolated environment.</param>
    /// <returns>The asynchronous result of the <paramref name="task"/>.</returns>
    /// <exception cref="ApiProxyException">if the <paramref name="task"/> throws an exception.</exception>
    Task ExecuteAsync(DatabaseRequestTask<TDbContext> task);

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
    Task<TResult> ExecuteReadOnlyAsync<TResult>(ReadOnlyDatabaseRequestTask<TDbContext, TResult> task);

    /// <summary>
    /// Executes the specified asynchronous <paramref name="task"/> in an isolated database transaction with automatic error handling.
    /// </summary>
    /// <typeparam name="TResult">The result of the <paramref name="task"/>.</typeparam>
    /// <param name="task">The asynchronous Task to be executed in the isolated environment.</param>
    /// <exception cref="ApiProxyException">if the <paramref name="task"/> throws an exception.</exception>
    Task<TResult> ExecuteAsync<TResult>(DatabaseRequestTask<TDbContext, TResult> task);
}