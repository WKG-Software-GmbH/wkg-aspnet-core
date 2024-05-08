using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using Wkg.AspNetCore.Configuration;
using Wkg.AspNetCore.Exceptions;
using Wkg.AspNetCore.RequestActions;
using Wkg.AspNetCore.TransactionManagement;

namespace Wkg.AspNetCore.Abstractions.Managers;

/// <summary>
/// Provides a base class for API managers that require database access.
/// </summary>
/// <typeparam name="TDbContext">The type of the database context.</typeparam>
/// <remarks>
/// Initializes a new instance of the <see cref="DatabaseManager{TDbContext}"/> class.
/// </remarks>
/// <param name="dbContextDescriptor">The DI descriptor of the database context.</param>
public abstract partial class DatabaseManager<TDbContext>(IDbContextDescriptor dbContextDescriptor) : ErrorHandlingManager where TDbContext : DbContext
{
    private TDbContext? _dbContext;

    internal TDbContext DbContext => _dbContext ??= dbContextDescriptor.GetDbContext<TDbContext>();

    private bool _isIsolated = false;

    /// <summary>
    /// Indicates whether this manager is allowed to manage transactions.
    /// </summary>
    /// <remarks>
    /// The value of this property may be set by unit tests to disable transaction management, e.g. to allow the unit test runner to roll back transactions after each test.
    /// </remarks>
    internal protected bool TransactionManagementAllowed { get; private set; } = true;

    private TransactionalContinuationType _continuationType = TransactionalContinuationType.Commit;

    /// <summary>
    /// Gets or sets the <see cref="IsolationLevel"/> to be used for all transactions of this manager.
    /// </summary>
    internal protected IsolationLevel TransactionIsolationLevel { get; init; } = DatabaseTransactionDefaults.DefaultIsolationLevel;

    /// <summary>
    /// Executes the specified <paramref name="action"/> in an isolated readonly database transaction with automatic error handling.
    /// </summary>
    /// <remarks>
    /// The transaction is automatically rolled back after the action has been executed.
    /// </remarks>
    /// <param name="action">The action to be executed in the isolated environment.</param>
    /// <exception cref="ApiProxyException">if the <paramref name="action"/> throws an exception.</exception>
    /// <exception cref="ConcurrencyViolationException">if <paramref name="action"/> is a <see cref="Task"/>-returning method.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal protected IActionResult InReadOnlyTransaction(ReadOnlyDatabaseRequestAction<TDbContext, IActionResult> action) =>
        InReadOnlyTransaction<IActionResult>(action);

    /// <summary>
    /// Executes the specified <paramref name="action"/> in an isolated database transaction with automatic error handling.
    /// </summary>
    /// <param name="action">The action to be executed in the isolated environment.</param>
    /// <exception cref="ApiProxyException">if the <paramref name="action"/> throws an exception.</exception>
    /// <exception cref="ConcurrencyViolationException">if <paramref name="action"/> is a <see cref="Task"/>-returning method.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal protected IActionResult InTransaction(DatabaseRequestAction<TDbContext, IActionResult> action) =>
        InTransaction<IActionResult>(action);

    /// <summary>
    /// Executes the specified <paramref name="action"/> in an isolated readonly database transaction with automatic error handling.
    /// </summary>
    /// <remarks>
    /// The transaction is automatically rolled back after the action has been executed.
    /// </remarks>
    /// <param name="action">The action to be executed in the isolated environment.</param>
    /// <exception cref="ApiProxyException">if the <paramref name="action"/> throws an exception.</exception>
    /// <exception cref="ConcurrencyViolationException">if <paramref name="action"/> is a <see cref="Task"/>-returning method.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal protected void InReadOnlyTransaction(ReadOnlyDatabaseRequestAction<TDbContext> action) => InTransaction(dbContext =>
    {
        action.Invoke(dbContext);
        return new TransactionalContinuation<VoidResult>(TransactionalContinuationType.Rollback, default);
    });

    /// <summary>
    /// Executes the specified <paramref name="action"/> in an isolated database transaction with automatic error handling.
    /// </summary>
    /// <param name="action">The action to be executed in the isolated environment.</param>
    /// <exception cref="ApiProxyException">if the <paramref name="action"/> throws an exception.</exception>
    /// <exception cref="ConcurrencyViolationException">if <paramref name="action"/> is a <see cref="Task"/>-returning method.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal protected void InTransaction(DatabaseRequestAction<TDbContext> action) => InTransaction(dbContext =>
    {
        ITransactionalContinuation continuation = action.Invoke(dbContext);
        return new TransactionalContinuation<VoidResult>(continuation.NextAction, default);
    });

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
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal protected TResult InReadOnlyTransaction<TResult>(ReadOnlyDatabaseRequestAction<TDbContext, TResult> action) =>
        InTransaction(dbContext => Rollback(action.Invoke(dbContext)));

    /// <summary>
    /// Executes the specified <paramref name="action"/> in an isolated database transaction with automatic error handling.
    /// </summary>
    /// <typeparam name="TResult">The result of the <paramref name="action"/>.</typeparam>
    /// <param name="action">The action to be executed in the isolated environment.</param>
    /// <returns>The result of the <paramref name="action"/>.</returns>
    /// <exception cref="ApiProxyException">if the <paramref name="action"/> throws an exception.</exception>
    /// <exception cref="ConcurrencyViolationException">if <paramref name="action"/> is a <see cref="Task"/>-returning method.</exception>
    internal protected TResult InTransaction<TResult>(DatabaseRequestAction<TDbContext, TResult> action)
    {
        if (typeof(TResult).IsAssignableTo(typeof(Task)))
        {
            ThrowConcurrencyViolation_AsyncInSyncContext();
        }

        // if we are running already in an isolated context, return immediately
        // (enables recursion)
        if (_isIsolated && DbContext.Database.CurrentTransaction is not null)
        {
            ITransactionalContinuation<TResult> result = action.Invoke(DbContext);
            _continuationType |= result.NextAction;
            return result.Result;
        }

        IDbContextTransaction? transaction = null;
        try
        {
            transaction = DbContext.Database.BeginTransaction(TransactionIsolationLevel);

            _isIsolated = true;
            ITransactionalContinuation<TResult> result = action.Invoke(DbContext);
            _continuationType |= result.NextAction;

            // only commit or rollback if we are the outermost transaction and if we are allowed to manage transactions
            if (TransactionManagementAllowed)
            {
                if (_continuationType is TransactionalContinuationType.Commit)
                {
                    transaction.Commit();
                }
                else
                {
                    transaction.Rollback();
                }
            }
            return result.Result;
        }
        catch (Exception e)
        {
            throw AfterHandled(e, transaction);
        }
        finally
        {
            if (TransactionManagementAllowed)
            {
                transaction?.Dispose();
                _isIsolated = false;
                _continuationType = TransactionalContinuationType.Commit;
            }
        }
    }

    [DoesNotReturn]
    private void ThrowConcurrencyViolation_AsyncInSyncContext()
    {
        ConcurrencyViolationException ex = new(SR.ConcurrencyViolation_AsyncInSyncContext);
        ExceptionDispatchInfo.SetCurrentStackTrace(ex);
        throw AfterHandled(ex);
    }

    /// <summary>
    /// Executes the specified asynchronous <paramref name="task"/> in an isolated readonly database transaction with automatic error handling.
    /// </summary>
    /// <remarks>
    /// The transaction is automatically rolled back after the action has been executed.
    /// </remarks>
    /// <param name="task">The action to be executed in the isolated environment.</param>
    /// <returns>The asynchronous result of the <paramref name="task"/>.</returns>
    /// <exception cref="ApiProxyException">if the <paramref name="task"/> throws an exception.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal protected Task<IActionResult> InReadOnlyTransactionAsync(ReadOnlyDatabaseRequestTask<TDbContext, IActionResult> task) =>
        InReadOnlyTransactionAsync<IActionResult>(task);

    /// <summary>
    /// Executes the specified asynchronous <paramref name="task"/> in an isolated database transaction with automatic error handling.
    /// </summary>
    /// <param name="task">The action to be executed in the isolated environment.</param>
    /// <returns>The asynchronous result of the <paramref name="task"/>.</returns>
    /// <exception cref="ApiProxyException">if the <paramref name="task"/> throws an exception.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal protected Task<IActionResult> InTransactionAsync(DatabaseRequestTask<TDbContext, IActionResult> task) =>
        InTransactionAsync<IActionResult>(task);

    /// <summary>
    /// Executes the specified asynchronous <paramref name="task"/> in an isolated readonly database transaction with automatic error handling.
    /// </summary>
    /// <param name="task">The action to be executed in the isolated environment.</param>
    /// <returns>The asynchronous result of the <paramref name="task"/>.</returns>
    /// <exception cref="ApiProxyException">if the <paramref name="task"/> throws an exception.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal protected Task InReadOnlyTransactionAsync(ReadOnlyDatabaseRequestTask<TDbContext> task) => InTransactionAsync<VoidResult>(async dbContext =>
    {
        await task.Invoke(dbContext);
        return new TransactionalContinuation<VoidResult>(TransactionalContinuationType.Rollback, default);
    });

    /// <summary>
    /// Executes the specified asynchronous <paramref name="task"/> in an isolated database transaction with automatic error handling.
    /// </summary>
    /// <param name="task">The action to be executed in the isolated environment.</param>
    /// <returns>The asynchronous result of the <paramref name="task"/>.</returns>
    /// <exception cref="ApiProxyException">if the <paramref name="task"/> throws an exception.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal protected Task InTransactionAsync(DatabaseRequestTask<TDbContext> task) => InTransactionAsync<VoidResult>(async dbContext =>
    {
        ITransactionalContinuation continuation = await task.Invoke(dbContext);
        return new TransactionalContinuation<VoidResult>(continuation.NextAction, default);
    });

    /// <summary>
    /// Executes the specified asynchronous <paramref name="task"/> in an isolated readonly database transaction with automatic error handling.
    /// </summary>
    /// <typeparam name="TResult">The result of the <paramref name="task"/>.</typeparam>
    /// <param name="task">The asynchronous Task to be executed in the isolated environment.</param>
    /// <exception cref="ApiProxyException">if the <paramref name="task"/> throws an exception.</exception>
    internal protected Task<TResult> InReadOnlyTransactionAsync<TResult>(ReadOnlyDatabaseRequestTask<TDbContext, TResult> task) => InTransactionAsync(async dbContext =>
    {
        TResult result = await task.Invoke(dbContext);
        return Rollback(result);
    });

    /// <summary>
    /// Executes the specified asynchronous <paramref name="task"/> in an isolated database transaction with automatic error handling.
    /// </summary>
    /// <typeparam name="TResult">The result of the <paramref name="task"/>.</typeparam>
    /// <param name="task">The asynchronous Task to be executed in the isolated environment.</param>
    /// <exception cref="ApiProxyException">if the <paramref name="task"/> throws an exception.</exception>
    internal protected async Task<TResult> InTransactionAsync<TResult>(DatabaseRequestTask<TDbContext, TResult> task)
    {
        // if we are running already in an isolated context, return immediately
        // (enables recursion)
        if (_isIsolated && DbContext.Database.CurrentTransaction is not null)
        {
            ITransactionalContinuation<TResult> result = await task.Invoke(DbContext);
            _continuationType |= result.NextAction;
            return result.Result;
        }

        IDbContextTransaction? transaction = null;
        try
        {
            // only create a new transaction if we are not running in an isolated context already
            transaction = await DbContext.Database.BeginTransactionAsync(TransactionIsolationLevel);

            // we are now running in an isolated context (prevents recursion)
            _isIsolated = true;

            // execute the client code and evaluate the result (commit or rollback)
            ITransactionalContinuation<TResult> result = await task.Invoke(DbContext);
            _continuationType |= result.NextAction;

            // only commit or rollback if we are the outermost transaction and if we are allowed to manage transactions
            // (if we are not allowed to manage transactions, we are running in a unit test, and the unit test runner
            // will take care of rolling back the transaction)
            if (TransactionManagementAllowed)
            {
                if (_continuationType is TransactionalContinuationType.Commit)
                {
                    await transaction.CommitAsync();
                }
                else
                {
                    await transaction.RollbackAsync();
                }
            }

            // return the actual result of the client code (API response)
            return result.Result;
        }
        catch (Exception e)
        {
            throw await AfterHandledAsync(e, transaction);
        }
        finally
        {
            // clean up (only if we are the outermost transaction and not running in a unit test)
            if (TransactionManagementAllowed)
            {
                if (transaction is not null)
                {
                    await transaction.DisposeAsync();
                }
                _isIsolated = false;
                _continuationType = TransactionalContinuationType.Commit;
            }
        }
    }

    /// <summary>
    /// Handles the intercepted <see cref="Exception"/>, attempts to rollback the current transaction and returns a new <see cref="ApiProxyException"/> to be rethrown.
    /// </summary>
    /// <param name="e">The intercepted <see cref="Exception"/>.</param>
    /// <param name="transaction">The current transaction.</param>
    /// <returns>A new <see cref="ApiProxyException"/> to be rethrown.</returns>
    internal protected virtual ApiProxyException AfterHandled(Exception e, IDbContextTransaction? transaction)
    {
        if (TransactionManagementAllowed && transaction is not null)
        {
            try
            {
                transaction.Rollback();
            }
            catch (Exception rollbackFailure)
            {
                e = new AggregateException(e, rollbackFailure);
            }
        }
        return AfterHandled(e);
    }

    /// <summary>
    /// Handles the intercepted <see cref="Exception"/>, attempts to rollback the current transaction and returns a new <see cref="ApiProxyException"/> to be rethrown.
    /// </summary>
    /// <param name="e">The intercepted <see cref="Exception"/>.</param>
    /// <param name="transaction">The current transaction.</param>
    /// <returns>A new <see cref="ApiProxyException"/> to be rethrown.</returns>
    internal protected virtual async ValueTask<ApiProxyException> AfterHandledAsync(Exception e, IDbContextTransaction? transaction)
    {
        if (TransactionManagementAllowed && transaction is not null)
        {
            try
            {
                await transaction.RollbackAsync();
            }
            catch (Exception rollbackFailure)
            {
                e = new AggregateException(e, rollbackFailure);
            }
        }
        return AfterHandled(e);
    }
}
