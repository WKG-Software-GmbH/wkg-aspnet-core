﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Data;
using System.Runtime.CompilerServices;
using Wkg.AspNetCore.Exceptions;
using Wkg.AspNetCore.RequestActions;
using Wkg.AspNetCore.TransactionManagement;

namespace Wkg.AspNetCore.Controllers;

/// <summary>
/// Provides a base class for API controllers that require database access.
/// </summary>
/// <typeparam name="TDbContext">The type of the database context.</typeparam>
public abstract partial class DatabaseController<TDbContext> : ErrorHandlingController where TDbContext : DbContext
{
    private protected TDbContext DbContext { get; }

    private bool _isIsolated = false;

    /// <summary>
    /// Indicates whether this controller is allowed to manage transactions.
    /// </summary>
    /// <remarks>
    /// The value of this property may be set by unit tests to disable transaction management, e.g. to allow the unit test runner to roll back transactions after each test.
    /// </remarks>
    protected bool TransactionManagementAllowed { get; private set; } = true;

    private TransactionalContinuationType _continuationType = TransactionalContinuationType.Commit;

    private static IsolationLevel TransactionIsolationLevel => IsolationLevel.ReadCommitted;

    /// <summary>
    /// Initializes a new instance of the <see cref="DatabaseController{TDbContext}"/> class.
    /// </summary>
    /// <param name="dbContext">The database context.</param>
    protected DatabaseController(TDbContext dbContext) => DbContext = dbContext;

    /// <summary>
    /// Executes the specified <paramref name="action"/> in an isolated database transaction with automatic error handling.
    /// </summary>
    /// <param name="action">The action to be executed in the isolated environment.</param>
    /// <exception cref="ApiProxyException">if the <paramref name="action"/> throws an exception.</exception>
    /// <exception cref="InvalidOperationException">if the <see cref="ControllerBase.ModelState"/> is invalid.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected IActionResult InTransaction(DatabaseRequestAction<TDbContext, IActionResult> action) =>
        InTransaction<IActionResult>(action);

    /// <summary>
    /// Executes the specified <paramref name="action"/> in an isolated database transaction with automatic error handling.
    /// </summary>
    /// <param name="action">The action to be executed in the isolated environment.</param>
    /// <exception cref="ApiProxyException">if the <paramref name="action"/> throws an exception.</exception>
    /// <exception cref="InvalidOperationException">if the <see cref="ControllerBase.ModelState"/> is invalid.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected void InTransaction(DatabaseRequestAction<TDbContext> action) => InTransaction<IActionResult>(dbContext =>
    {
        ITransactionalContinuation continuation = action.Invoke(dbContext);
        return new TransactionalContinuation<IActionResult>(continuation.NextAction, Ok());
    });

    /// <summary>
    /// Executes the specified <paramref name="action"/> in an isolated database transaction with automatic error handling.
    /// </summary>
    /// <typeparam name="TResult">The result of the <paramref name="action"/>.</typeparam>
    /// <param name="action">The action to be executed in the isolated environment.</param>
    /// <returns>The result of the <paramref name="action"/>.</returns>
    /// <exception cref="ApiProxyException">if the <paramref name="action"/> throws an exception.</exception>
    /// <exception cref="InvalidOperationException">if the <see cref="ControllerBase.ModelState"/> is invalid.</exception>
    protected TResult InTransaction<TResult>(DatabaseRequestAction<TDbContext, TResult> action)
    {
        // if we are running already in an isolated context, return immediately
        // (enables recursion)
        if (_isIsolated && DbContext.Database.CurrentTransaction is not null)
        {
            ITransactionalContinuation<TResult> result = action.Invoke(DbContext);
            _continuationType |= result.NextAction;
            return result.Result;
        }

        // Validates the Model State
        AssertModelState();

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

    /// <summary>
    /// Executes the specified asynchronous <paramref name="task"/> in an isolated database transaction with automatic error handling.
    /// </summary>
    /// <param name="task">The action to be executed in the isolated environment.</param>
    /// <returns>The asynchronous result of the <paramref name="task"/>.</returns>
    /// <exception cref="ApiProxyException">if the <paramref name="task"/> throws an exception.</exception>
    /// <exception cref="InvalidOperationException">if the <see cref="ControllerBase.ModelState"/> is invalid.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected Task<IActionResult> InTransactionAsync(DatabaseRequestTask<TDbContext, IActionResult> task) =>
        InTransactionAsync<IActionResult>(task);

    /// <summary>
    /// Executes the specified asynchronous <paramref name="task"/> in an isolated database transaction with automatic error handling.
    /// </summary>
    /// <param name="task">The action to be executed in the isolated environment.</param>
    /// <returns>The asynchronous result of the <paramref name="task"/>.</returns>
    /// <exception cref="ApiProxyException">if the <paramref name="task"/> throws an exception.</exception>
    /// <exception cref="InvalidOperationException">if the <see cref="ControllerBase.ModelState"/> is invalid.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected Task InTransactionAsync(DatabaseRequestTask<TDbContext> task) => InTransactionAsync(async dbContext =>
    {
        ITransactionalContinuation continuation = await task.Invoke(dbContext);
        return new TransactionalContinuation<IActionResult>(continuation.NextAction, Ok());
    });

    /// <summary>
    /// Executes the specified asynchronous <paramref name="task"/> in an isolated database transaction with automatic error handling.
    /// </summary>
    /// <typeparam name="TResult">The result of the <paramref name="task"/>.</typeparam>
    /// <param name="task">The asynchronous Task to be executed in the isolated environment.</param>
    /// <exception cref="ApiProxyException">if the <paramref name="task"/> throws an exception.</exception>
    /// <exception cref="InvalidOperationException"></exception>
    protected async Task<TResult> InTransactionAsync<TResult>(DatabaseRequestTask<TDbContext, TResult> task)
    {
        // if we are running already in an isolated context, return immediately
        // (enables recursion)
        if (_isIsolated && DbContext.Database.CurrentTransaction is not null)
        {
            ITransactionalContinuation<TResult> result = await task.Invoke(DbContext);
            _continuationType |= result.NextAction;
            return result.Result;
        }

        AssertModelState();

        IDbContextTransaction? transaction = null;
        try
        {
            transaction = await DbContext.Database.BeginTransactionAsync(TransactionIsolationLevel);

            _isIsolated = true;

            ITransactionalContinuation<TResult> result = await task.Invoke(DbContext);
            _continuationType |= result.NextAction;

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
            return result.Result;
        }
        catch (Exception e)
        {
            throw await AfterHandledAsync(e, transaction);
        }
        finally
        {
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
    protected virtual ApiProxyException AfterHandled(Exception e, IDbContextTransaction? transaction)
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
    protected virtual async ValueTask<ApiProxyException> AfterHandledAsync(Exception e, IDbContextTransaction? transaction)
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