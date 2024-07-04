using Microsoft.AspNetCore.Mvc;
using System.Runtime.CompilerServices;
using Wkg.AspNetCore.TransactionManagement;
using Wkg.AspNetCore.Transactions.Actions;

namespace Wkg.AspNetCore.Transactions;

partial class TransactionScope<TDbContext>
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IActionResult ExecuteReadOnly(ReadOnlyDatabaseRequestAction<TDbContext, IActionResult> action) =>
        ExecuteReadOnly<IActionResult>(action);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IActionResult Execute(DatabaseRequestAction<TDbContext, IActionResult> action) =>
        Execute<IActionResult>(action);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void ExecuteReadOnly(ReadOnlyDatabaseRequestAction<TDbContext> action) => Execute((dbContext, transaction) =>
    {
        action.Invoke(dbContext);
        return transaction.Rollback<VoidResult>(default);
    });

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Execute(DatabaseRequestAction<TDbContext> action) => Execute((dbContext, transaction) =>
    {
        ITransactionalContinuation continuation = action.Invoke(dbContext, transaction);
        return new TransactionalContinuation<VoidResult>(continuation.NextAction, default);
    });

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TResult ExecuteReadOnly<TResult>(ReadOnlyDatabaseRequestAction<TDbContext, TResult> action) =>
        Execute((dbContext, transaction) => transaction.Rollback(action.Invoke(dbContext)));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TResult Execute<TResult>(DatabaseRequestAction<TDbContext, TResult> action) =>
        RunInScope(action);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task<IActionResult> ExecuteReadOnlyAsync(ReadOnlyDatabaseRequestTask<TDbContext, IActionResult> task) =>
        ExecuteReadOnlyAsync<IActionResult>(task);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task<IActionResult> ExecuteAsync(DatabaseRequestTask<TDbContext, IActionResult> task) =>
        ExecuteAsync<IActionResult>(task);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task ExecuteReadOnlyAsync(ReadOnlyDatabaseRequestTask<TDbContext> task) => ExecuteAsync(async (dbContext, transaction) =>
    {
        await task.Invoke(dbContext);
        return transaction.Rollback<VoidResult>(default);
    });

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task ExecuteAsync(DatabaseRequestTask<TDbContext> task) => ExecuteAsync<VoidResult>(async (dbContext, transaction) =>
    {
        ITransactionalContinuation continuation = await task.Invoke(dbContext, transaction);
        return new TransactionalContinuation<VoidResult>(continuation.NextAction, default);
    });

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task<TResult> ExecuteReadOnlyAsync<TResult>(ReadOnlyDatabaseRequestTask<TDbContext, TResult> task) => ExecuteAsync(async (dbContext, transaction) =>
    {
        TResult result = await task.Invoke(dbContext);
        return transaction.Rollback(result);
    });

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task<TResult> ExecuteAsync<TResult>(DatabaseRequestTask<TDbContext, TResult> task) =>
        RunInScopeAsync(task);
}
