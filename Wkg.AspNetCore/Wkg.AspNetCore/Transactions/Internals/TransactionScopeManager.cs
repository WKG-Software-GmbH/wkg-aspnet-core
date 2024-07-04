using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Data;

namespace Wkg.AspNetCore.Transactions.Internals;

internal class TransactionScopeManager<TDbContext>(IDbContextDescriptor dbContextDescriptor, TransactionManagerOptions options, IServiceProvider serviceProvider) : ITransactionScopeManager<TDbContext>
    where TDbContext : DbContext
{
    IsolationLevel ITransactionScopeManager<TDbContext>.TransactionIsolationLevel => options.TransactionIsolationLevel;

    IDbContextDescriptor ITransactionScopeManager<TDbContext>.DbContextDescriptor => dbContextDescriptor;

    public ITransactionScope<TDbContext> CreateScope() =>
        serviceProvider.GetRequiredService<ITransactionScope<TDbContext>>();
}
