using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Wkg.AspNetCore.Exceptions;
using Wkg.AspNetCore.Transactions.Actions;

namespace Wkg.AspNetCore.Transactions;

public interface ITransactionScope<TDbContext> : IAsyncDisposable where TDbContext : DbContext
{
    /// <summary>
    /// Executes the specified <paramref name="action"/> in an isolated readonly database transaction with automatic error handling.
    /// </summary>
    /// <remarks>
    /// The transaction is automatically rolled back after the action has been executed.
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
    /// The transaction is automatically rolled back after the action has been executed.
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
    /// The transaction is automatically rolled back after the action has been executed.
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
    /// The transaction is automatically rolled back after the action has been executed.
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