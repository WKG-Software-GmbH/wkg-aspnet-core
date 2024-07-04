using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Data;

namespace Wkg.AspNetCore.Transactions;

public interface ITransactionScopeManager<TDbContext> where TDbContext : DbContext
{
    /// <summary>
    /// Gets or sets the <see cref="IsolationLevel"/> to be used for all transactions of this manager.
    /// </summary>
    internal IsolationLevel TransactionIsolationLevel { get; }

    internal IDbContextDescriptor DbContextDescriptor { get; }

    public ITransactionScope<TDbContext> CreateScope();
}

internal class TransactionScopeManager<TDbContext>(IDbContextDescriptor dbContextDescriptor, TransactionManagerOptions options, IServiceProvider serviceProvider) : ITransactionScopeManager<TDbContext>
    where TDbContext : DbContext
{
    IsolationLevel ITransactionScopeManager<TDbContext>.TransactionIsolationLevel => options.TransactionIsolationLevel;

    IDbContextDescriptor ITransactionScopeManager<TDbContext>.DbContextDescriptor => dbContextDescriptor;

    public ITransactionScope<TDbContext> CreateScope() => 
        serviceProvider.GetRequiredService<ITransactionScope<TDbContext>>();
}

internal record TransactionManagerOptions(IsolationLevel TransactionIsolationLevel);