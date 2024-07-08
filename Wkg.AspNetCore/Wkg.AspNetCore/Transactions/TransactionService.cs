using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Data;

namespace Wkg.AspNetCore.Transactions;

internal class TransactionService<TDbContext>(ITransaction<TDbContext> defaultScope, TransactionServiceOptions options, IServiceProvider serviceProvider) : ITransactionService<TDbContext>
    where TDbContext : DbContext
{
    public ITransaction<TDbContext> Scoped => defaultScope;

    public IsolationLevel IsolationLevel { get; set; } = options.TransactionIsolationLevel;

    public ITransaction<TDbContext> BeginTransaction(IsolationLevel? isolationLevel = default)
    {
        AsyncServiceScope scope = serviceProvider.CreateAsyncScope();
        ITransaction<TDbContext> transaction = scope.ServiceProvider.GetRequiredService<ITransaction<TDbContext>>();
        transaction.IsolationLevel = isolationLevel ?? IsolationLevel;
        transaction.SlavedScope = scope;
        return transaction;
    }
}