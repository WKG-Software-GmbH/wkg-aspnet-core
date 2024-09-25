using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.ExceptionServices;
using Wkg.AspNetCore.ErrorHandling;
using Wkg.AspNetCore.Transactions.Continuations;
using Wkg.AspNetCore.Transactions.Delegates;

namespace Wkg.AspNetCore.Transactions;

internal partial class Transaction<TDbContext>(TDbContext dbContext, IErrorSentry errorSentry, TransactionServiceOptions options) 
    : ITransaction<TDbContext> where TDbContext : DbContext
{
    private static readonly ScopedTransaction s_scopedTransactionInstance = new();
    private IDbContextTransaction? _transaction;
    private bool _isDisposed;
    private bool _isGuarded;
    private IsolationLevel _isolationLevel = options.TransactionIsolationLevel;

    private AsyncServiceScope? _slavedScope;

    internal TDbContext DbContext => dbContext;

    public TransactionState State { get; private set; } = TransactionState.ReadOnly;

    IsolationLevel ITransaction<TDbContext>.IsolationLevel 
    {
        get => _isolationLevel;
        set
        {
            if (_transaction is not null)
            {
                ThrowInvalidOperationException_TransactionAlreadyStarted();
            }
            _isolationLevel = value;
        }
    }

    AsyncServiceScope? ITransaction<TDbContext>.SlavedScope 
    { 
        get => _slavedScope; 
        set => _slavedScope = value; 
    }

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
            _transaction = DbContext.Database.BeginTransaction(_isolationLevel);
        }
        else if (!ReferenceEquals(_transaction, DbContext.Database.CurrentTransaction))
        {
            ThrowConsistencyViolation_ManualTransactionManagement();
        }

        // if we are running already in a guarded context, return immediately
        // (enables recursion and prevents double error handling)
        if (_isGuarded)
        {
            IDeferredTransactionState<TResult> result = action.Invoke(DbContext, s_scopedTransactionInstance);
            State |= result.NextState;
            return result.Result;
        }

        try
        {
            _isGuarded = true;
            IDeferredTransactionState<TResult> result = action.Invoke(DbContext, s_scopedTransactionInstance);
            State |= result.NextState;
            return result.Result;
        }
        catch (Exception e)
        {
            State |= TransactionState.Exception;
            throw errorSentry.AfterHandled(e);
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
            _transaction = await DbContext.Database.BeginTransactionAsync(_isolationLevel);
        }
        else if (!ReferenceEquals(_transaction, DbContext.Database.CurrentTransaction))
        {
            ThrowConsistencyViolation_ManualTransactionManagement();
        }

        // if we are running already in a guarded context, return immediately
        // (enables recursion and prevents double error handling)
        if (_isGuarded)
        {
            IDeferredTransactionState<TResult> result = await task.Invoke(DbContext, s_scopedTransactionInstance);
            State |= result.NextState;
            return result.Result;
        }

        try
        {
            _isGuarded = true;
            IDeferredTransactionState<TResult> result = await task.Invoke(DbContext, s_scopedTransactionInstance);
            State |= result.NextState;
            return result.Result;
        }
        catch (Exception e)
        {
            State |= TransactionState.Exception;
            throw errorSentry.AfterHandled(e);
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
            if (State is TransactionState.Commit)
            {
                await CommitAsync(_transaction);
            }
            else
            {
                await RollbackAsync(_transaction);
            }
            State |= TransactionState.Finalized;
            _transaction.Dispose();
        }
        if (_slavedScope.HasValue)
        {
            await _slavedScope.Value.DisposeAsync();
        }
    }

    [DoesNotReturn]
    private void ThrowConcurrencyViolation_AsyncInSyncContext()
    {
        ConcurrencyViolationException ex = new(SR.ConcurrencyViolation_AsyncInSyncContext);
        ExceptionDispatchInfo.SetCurrentStackTrace(ex);
        throw errorSentry.AfterHandled(ex);
    }

    [DoesNotReturn]
    private void ThrowConsistencyViolation_ManualTransactionManagement()
    {
        ConcurrencyViolationException ex = new(SR.ConsistencyViolation_ManualTransactionManagement);
        ExceptionDispatchInfo.SetCurrentStackTrace(ex);
        throw errorSentry.AfterHandled(ex);
    }

    [DoesNotReturn]
    private void ThrowInvalidOperationException_TransactionAlreadyStarted()
    {
        InvalidOperationException ex = new(SR.InvalidOperation_TransactionAlreadyStarted);
        ExceptionDispatchInfo.SetCurrentStackTrace(ex);
        throw errorSentry.AfterHandled(ex);
    }
}