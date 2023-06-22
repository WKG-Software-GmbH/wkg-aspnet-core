using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Data;
using Wkg.AspNetCore.Exceptions;
using Wkg.AspNetCore.RequestActions;
using Wkg.AspNetCore.TransactionManagement;

namespace Wkg.AspNetCore.Controllers;

public abstract partial class DatabaseController<TDbContext> : ErrorHandlingController where TDbContext : DbContext
{
    private protected TDbContext DbContext { get; }

    private bool _isIsolated = false;

    protected bool TransactionManagementAllowed { get; private set; } = true;

    private TransactionalContinuationType _continuationType = TransactionalContinuationType.Commit;

    private static IsolationLevel TransactionIsolationLevel => IsolationLevel.ReadCommitted;

    public DatabaseController(TDbContext dbContext) => DbContext = dbContext;

    /// <summary>
    /// Executes the specified <paramref name="requestAction"/> in an isolated database transaction with automatic error handling.
    /// </summary>
    protected IActionResult InTransaction(DatabaseRequestAction<TDbContext, IActionResult> requestAction) =>
        InTransaction<IActionResult>(requestAction);

    /// <summary>
    /// Executes the specified <paramref name="requestAction"/> in an isolated database transaction with automatic error handling.
    /// </summary>
    protected void InTransaction(DatabaseRequestAction<TDbContext> requestAction) => InTransaction<IActionResult>(dbContext =>
    {
        ITransactionalContinuation continuation = requestAction.Invoke(dbContext);
        return new TransactionalContinuation<IActionResult>(continuation.NextAction, Ok());
    });

    /// <summary>
    /// Executes the specified <paramref name="requestAction"/> in an isolated database transaction with automatic error handling.
    /// </summary>
    /// <typeparam name="TResult">The result of the <paramref name="requestAction"/>.</typeparam>
    /// <param name="requestAction">The action to be executed in the isolated environment.</param>
    /// <exception cref="InvalidOperationException"></exception>
    protected TResult InTransaction<TResult>(DatabaseRequestAction<TDbContext, TResult> requestAction)
    {
        // if we are running already in an isolated context, return immediately
        // (enables recursion)
        if (_isIsolated && DbContext.Database.CurrentTransaction is not null)
        {
            ITransactionalContinuation<TResult> result = requestAction.Invoke(DbContext);
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
            ITransactionalContinuation<TResult> result = requestAction.Invoke(DbContext);
            _continuationType |= result.NextAction;

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
            }
        }
    }

    /// <summary>
    /// Executes the specified asynchronous <paramref name="asyncRequestAction"/> in an isolated database transaction with automatic error handling.
    /// </summary>
    /// <param name="asyncRequestAction">The action to be executed in the isolated environment.</param>
    protected Task<IActionResult> InTransactionAsync(DatabaseRequestTask<TDbContext, IActionResult> asyncRequestAction) =>
        InTransactionAsync<IActionResult>(asyncRequestAction);

    protected Task InTransactionAsync(DatabaseRequestTask<TDbContext> requestAction) => InTransactionAsync(async dbContext =>
    {
        ITransactionalContinuation continuation = await requestAction.Invoke(dbContext);
        return new TransactionalContinuation<IActionResult>(continuation.NextAction, Ok());
    });

    /// <summary>
    /// Executes the specified asynchronous <paramref name="asyncRequestAction"/> in an isolated database transaction with automatic error handling.
    /// </summary>
    /// <typeparam name="TResult">The result of the <paramref name="asyncRequestAction"/>.</typeparam>
    /// <param name="asyncRequestAction">The asynchronous Task to be executed in the isolated environment.</param>
    /// <exception cref="InvalidOperationException"></exception>
    protected async Task<TResult> InTransactionAsync<TResult>(DatabaseRequestTask<TDbContext, TResult> asyncRequestAction)
    {
        // if we are running already in an isolated context, return immediately
        // (enables recursion)
        if (_isIsolated && DbContext.Database.CurrentTransaction is not null)
        {
            ITransactionalContinuation<TResult> result = await asyncRequestAction.Invoke(DbContext);
            _continuationType |= result.NextAction;
            return result.Result;
        }

        AssertModelState();

        IDbContextTransaction? transaction = null;
        try
        {
            transaction = await DbContext.Database.BeginTransactionAsync(TransactionIsolationLevel);

            _isIsolated = true;

            ITransactionalContinuation<TResult> result = await asyncRequestAction.Invoke(DbContext);
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
            }
        }
    }

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
