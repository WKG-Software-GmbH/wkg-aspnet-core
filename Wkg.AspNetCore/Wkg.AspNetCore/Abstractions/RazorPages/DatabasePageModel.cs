using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Data;
using System.Runtime.CompilerServices;
using Wkg.AspNetCore.Abstractions.Managers;
using Wkg.AspNetCore.Exceptions;
using Wkg.AspNetCore.Interop;
using Wkg.AspNetCore.RequestActions;
using Wkg.AspNetCore.TransactionManagement;

namespace Wkg.AspNetCore.Abstractions.RazorPages;

/// <summary>
/// Provides a base class for Razor Pages that require database access.
/// </summary>
/// <typeparam name="TDbContext">The type of the database context.</typeparam>
public abstract class DatabasePageModel<TDbContext> : ErrorHandlingPageModel, IUnitTestTransactionHookProxy where TDbContext : DbContext
{
    private readonly DatabaseManager<TDbContext> _implementation;

    /// <summary>
    /// Initializes a new instance of the <see cref="DatabasePageModel{TDbContext}"/> class.
    /// </summary>
    /// <param name="dbContextDescriptor">The DI descriptor of the database context.</param>
    protected DatabasePageModel(IDbContextDescriptor dbContextDescriptor)
    {
        _implementation = new ProxiedDatabaseManager<TDbContext>(dbContextDescriptor)
        {
            Context = this
        };
    }

    #region Core API

    private protected TDbContext DbContext => _implementation.DbContext;

    /// <inheritdoc cref="DatabaseManager{TDbContext}.TransactionManagementAllowed"/>
    protected bool TransactionManagementAllowed => _implementation.TransactionManagementAllowed;

    /// <inheritdoc cref="DatabaseManager{TDbContext}.TransactionIsolationLevel"/>
    protected IsolationLevel TransactionIsolationLevel => _implementation.TransactionIsolationLevel;

    /// <inheritdoc cref="DatabaseManager{TDbContext}.InReadOnlyTransaction(ReadOnlyDatabaseRequestAction{TDbContext, IActionResult})"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected IActionResult InReadOnlyTransaction(ReadOnlyDatabaseRequestAction<TDbContext, IActionResult> action) =>
        _implementation.InReadOnlyTransaction(action);

    /// <inheritdoc cref="DatabaseManager{TDbContext}.InReadOnlyTransaction(ReadOnlyDatabaseRequestAction{TDbContext})"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected void InReadOnlyTransaction(ReadOnlyDatabaseRequestAction<TDbContext> action) =>
        _implementation.InReadOnlyTransaction(action);

    /// <inheritdoc cref="DatabaseManager{TDbContext}.InReadOnlyTransaction{TResult}(ReadOnlyDatabaseRequestAction{TDbContext, TResult})"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected TResult InReadOnlyTransaction<TResult>(ReadOnlyDatabaseRequestAction<TDbContext, TResult> action) =>
        _implementation.InReadOnlyTransaction(action);

    /// <inheritdoc cref="DatabaseManager{TDbContext}.InTransaction(DatabaseRequestAction{TDbContext, IActionResult})"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected IActionResult InTransaction(DatabaseRequestAction<TDbContext, IActionResult> action) =>
        _implementation.InTransaction(action);

    /// <inheritdoc cref="DatabaseManager{TDbContext}.InTransaction(DatabaseRequestAction{TDbContext})"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected void InTransaction(DatabaseRequestAction<TDbContext> action) =>
        _implementation.InTransaction(action);

    /// <inheritdoc cref="DatabaseManager{TDbContext}.InTransaction{TResult}(DatabaseRequestAction{TDbContext, TResult})"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected TResult InTransaction<TResult>(DatabaseRequestAction<TDbContext, TResult> action) =>
        _implementation.InTransaction(action);

    /// <inheritdoc cref="DatabaseManager{TDbContext}.InReadOnlyTransactionAsync(ReadOnlyDatabaseRequestTask{TDbContext, IActionResult})"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected Task<IActionResult> InReadOnlyTransactionAsync(ReadOnlyDatabaseRequestTask<TDbContext, IActionResult> task) =>
        _implementation.InReadOnlyTransactionAsync(task);

    /// <inheritdoc cref="DatabaseManager{TDbContext}.InReadOnlyTransactionAsync(ReadOnlyDatabaseRequestTask{TDbContext})"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected Task InReadOnlyTransactionAsync(ReadOnlyDatabaseRequestTask<TDbContext> task) =>
        _implementation.InReadOnlyTransactionAsync(task);

    /// <inheritdoc cref="DatabaseManager{TDbContext}.InReadOnlyTransactionAsync{TResult}(ReadOnlyDatabaseRequestTask{TDbContext, TResult})"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected Task<TResult> InReadOnlyTransactionAsync<TResult>(ReadOnlyDatabaseRequestTask<TDbContext, TResult> task) =>
        _implementation.InReadOnlyTransactionAsync(task);

    /// <inheritdoc cref="DatabaseManager{TDbContext}.InTransactionAsync(DatabaseRequestTask{TDbContext, IActionResult})"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected Task<IActionResult> InTransactionAsync(DatabaseRequestTask<TDbContext, IActionResult> task) =>
        _implementation.InTransactionAsync(task);

    /// <inheritdoc cref="DatabaseManager{TDbContext}.InTransactionAsync(DatabaseRequestTask{TDbContext})"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected Task InTransactionAsync(DatabaseRequestTask<TDbContext> task) =>
        _implementation.InTransactionAsync(task);

    /// <inheritdoc cref="DatabaseManager{TDbContext}.InTransactionAsync{TResult}(DatabaseRequestTask{TDbContext, TResult})"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected Task<TResult> InTransactionAsync<TResult>(DatabaseRequestTask<TDbContext, TResult> task) =>
        _implementation.InTransactionAsync(task);

    /// <inheritdoc cref="DatabaseManager{TDbContext}.AfterHandled(Exception, IDbContextTransaction?)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected virtual ApiProxyException AfterHandled(Exception e, IDbContextTransaction? transaction) =>
        _implementation.AfterHandled(e, transaction);

    /// <inheritdoc cref="DatabaseManager{TDbContext}.AfterHandledAsync(Exception, IDbContextTransaction?)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected virtual ValueTask<ApiProxyException> AfterHandledAsync(Exception e, IDbContextTransaction? transaction) => 
        _implementation.AfterHandledAsync(e, transaction);

    #endregion Core API
    #region ITransactionalContinuation

    /// <inheritdoc cref="DatabaseManager{TDbContext}.Commit"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected static ITransactionalContinuation Commit() =>
        DatabaseManager<TDbContext>.Commit();

    /// <inheritdoc cref="DatabaseManager{TDbContext}.Rollback"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected static ITransactionalContinuation Rollback() =>
        DatabaseManager<TDbContext>.Rollback();

    /// <inheritdoc cref="DatabaseManager{TDbContext}.Commit{TResult}"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected static ITransactionalContinuation<TResult> Commit<TResult>(TResult result) =>
        DatabaseManager<TDbContext>.Commit(result);

    /// <inheritdoc cref="DatabaseManager{TDbContext}.Rollback{TResult}"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected static ITransactionalContinuation<TResult> Rollback<TResult>(TResult result) =>
        DatabaseManager<TDbContext>.Rollback(result);

    #endregion ITransactionalContinuation

    IUnitTestTransactionHook IUnitTestTransactionHookProxy.TransactionHookImplementation => _implementation;
}
