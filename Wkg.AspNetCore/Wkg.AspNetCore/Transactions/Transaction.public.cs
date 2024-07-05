using Microsoft.AspNetCore.Mvc;
using System.Runtime.CompilerServices;
using Wkg.AspNetCore.Transactions.Actions;
using Wkg.AspNetCore.Transactions.Continuations;

namespace Wkg.AspNetCore.Transactions;

partial class Transaction<TDbContext>
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IActionResult RunReadOnly(ReadOnlyDatabaseRequestAction<TDbContext, IActionResult> action) =>
        RunReadOnly<IActionResult>(action);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public IActionResult Run(DatabaseRequestAction<TDbContext, IActionResult> action) =>
        Run<IActionResult>(action);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void RunReadOnly(ReadOnlyDatabaseRequestAction<TDbContext> action) => Run((dbContext, transaction) =>
    {
        action.Invoke(dbContext);
        return new TransactionalContinuation<VoidResult>(TransactionState.ReadOnly, default);
    });

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Run(DatabaseRequestAction<TDbContext> action) => Run((dbContext, transaction) =>
    {
        ITransactionalContinuation continuation = action.Invoke(dbContext, transaction);
        return new TransactionalContinuation<VoidResult>(continuation.NextAction, default);
    });

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TResult RunReadOnly<TResult>(ReadOnlyDatabaseRequestAction<TDbContext, TResult> action) => Run((dbContext, transaction) =>
    {
        TResult result = action.Invoke(dbContext);
        return new TransactionalContinuation<TResult>(TransactionState.ReadOnly, result);
    });

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public TResult Run<TResult>(DatabaseRequestAction<TDbContext, TResult> action) =>
        RunInScope(action);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task<IActionResult> RunReadOnlyAsync(ReadOnlyDatabaseRequestTask<TDbContext, IActionResult> task) =>
        RunReadOnlyAsync<IActionResult>(task);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task<IActionResult> RunAsync(DatabaseRequestTask<TDbContext, IActionResult> task) =>
        RunAsync<IActionResult>(task);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task RunReadOnlyAsync(ReadOnlyDatabaseRequestTask<TDbContext> task) => RunAsync(async (dbContext, transaction) =>
    {
        await task.Invoke(dbContext);
        return new TransactionalContinuation<VoidResult>(TransactionState.ReadOnly, default);
    });

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task RunAsync(DatabaseRequestTask<TDbContext> task) => RunAsync<VoidResult>(async (dbContext, transaction) =>
    {
        ITransactionalContinuation continuation = await task.Invoke(dbContext, transaction);
        return new TransactionalContinuation<VoidResult>(continuation.NextAction, default);
    });

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task<TResult> RunReadOnlyAsync<TResult>(ReadOnlyDatabaseRequestTask<TDbContext, TResult> task) => RunAsync<TResult>(async (dbContext, transaction) =>
    {
        TResult result = await task.Invoke(dbContext);
        return new TransactionalContinuation<TResult>(TransactionState.ReadOnly, result);
    });

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Task<TResult> RunAsync<TResult>(DatabaseRequestTask<TDbContext, TResult> task) =>
        RunInScopeAsync(task);
}
