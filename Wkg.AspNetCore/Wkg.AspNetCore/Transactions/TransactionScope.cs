using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.ExceptionServices;
using Wkg.AspNetCore.ErrorHandling;
using Wkg.AspNetCore.TransactionManagement;
using Wkg.AspNetCore.Transactions.Actions;

namespace Wkg.AspNetCore.Transactions;

internal partial class TransactionScope<TDbContext>(ITransactionScopeManager<TDbContext> transactionManager, IErrorHandler errorHandler) 
    : ITransactionScope<TDbContext> where TDbContext : DbContext
{
    private static readonly ScopedTransaction _scopedTransactionInstance = new();

    private TDbContext? _dbContext;
    private TransactionalContinuationType _continuationType = TransactionalContinuationType.ReadOnly;
    private IDbContextTransaction? _transaction;
    private bool _isDisposed;
    private bool _isGuarded;

    internal TDbContext DbContext => _dbContext ??= transactionManager.DbContextDescriptor.GetDbContext<TDbContext>();

    public IsolationLevel IsolationLevel => transactionManager.TransactionIsolationLevel;

    private protected TResult RunInScope<TResult>(DatabaseRequestAction<TDbContext, TResult> action)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);
        if (typeof(TResult).IsAssignableTo(typeof(Task)))
        {
            ThrowConcurrencyViolation_AsyncInSyncContext();
        }

        if (_transaction is null)
        {
            Debug.Assert(!_isGuarded);
            _transaction = DbContext.Database.BeginTransaction(IsolationLevel);
        }
        else if (!ReferenceEquals(_transaction, DbContext.Database.CurrentTransaction))
        {
            ThrowConsistencyViolation_ManualTransactionManagement();
        }

        // if we are running already in a guarded context, return immediately
        // (enables recursion and prevents double error handling)
        if (_isGuarded)
        {
            ITransactionalContinuation<TResult> result = action.Invoke(DbContext, _scopedTransactionInstance);
            _continuationType |= result.NextAction;
            return result.Result;
        }

        try
        {
            _isGuarded = true;
            ITransactionalContinuation<TResult> result = action.Invoke(DbContext, _scopedTransactionInstance);
            _continuationType |= result.NextAction;
            return result.Result;
        }
        catch (Exception e)
        {
            _continuationType = TransactionalContinuationType.ExceptionalRollback;
            throw errorHandler.AfterHandled(e);
        }
        finally
        {
            _isGuarded = false;
        }
    }

    private protected async Task<TResult> RunInScopeAsync<TResult>(DatabaseRequestTask<TDbContext, TResult> task)
    {
        ObjectDisposedException.ThrowIf(_isDisposed, this);

        if (_transaction is null)
        {
            Debug.Assert(!_isGuarded);
            _transaction = await DbContext.Database.BeginTransactionAsync(IsolationLevel);
        }
        else if (!ReferenceEquals(_transaction, DbContext.Database.CurrentTransaction))
        {
            ThrowConsistencyViolation_ManualTransactionManagement();
        }

        // if we are running already in a guarded context, return immediately
        // (enables recursion and prevents double error handling)
        if (_isGuarded)
        {
            ITransactionalContinuation<TResult> result = await task.Invoke(DbContext, _scopedTransactionInstance);
            _continuationType |= result.NextAction;
            return result.Result;
        }

        try
        {
            _isGuarded = true;
            ITransactionalContinuation<TResult> result = await task.Invoke(DbContext, _scopedTransactionInstance);
            _continuationType |= result.NextAction;
            return result.Result;
        }
        catch (Exception e)
        {
            _continuationType = TransactionalContinuationType.ExceptionalRollback;
            throw errorHandler.AfterHandled(e);
        }
        finally
        {
            _isGuarded = false;
        }
    }

    internal virtual Task CommitAsync(IDbContextTransaction transaction) => transaction.CommitAsync();

    internal virtual Task RollbackAsync(IDbContextTransaction transaction) => transaction.RollbackAsync();

    public async ValueTask DisposeAsync()
    {
        if (_isDisposed)
        {
            return;
        }

        _isDisposed = true;
        if (_transaction is not null)
        {
            if (_continuationType is TransactionalContinuationType.Commit)
            {
                await CommitAsync(_transaction);
            }
            else
            {
                await RollbackAsync(_transaction);
            }
            _transaction.Dispose();
        }
    }

    [DoesNotReturn]
    private void ThrowConcurrencyViolation_AsyncInSyncContext()
    {
        ConcurrencyViolationException ex = new(SR.ConcurrencyViolation_AsyncInSyncContext);
        ExceptionDispatchInfo.SetCurrentStackTrace(ex);
        throw errorHandler.AfterHandled(ex);
    }

    [DoesNotReturn]
    private void ThrowConsistencyViolation_ManualTransactionManagement()
    {
        ConcurrencyViolationException ex = new(SR.ConsistencyViolation_ManualTransactionManagement);
        ExceptionDispatchInfo.SetCurrentStackTrace(ex);
        throw errorHandler.AfterHandled(ex);
    }
}
