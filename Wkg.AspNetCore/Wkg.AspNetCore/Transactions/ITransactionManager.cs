using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Wkg.AspNetCore.ErrorHandling;

namespace Wkg.AspNetCore.Transactions;

public interface ITransactionManager
{
    internal ITransactionManager<TDbContext> GetInstance<TDbContext>() where TDbContext : DbContext;

    internal IErrorHandler ErrorHandler { get; }
}

internal class TransactionManager(IServiceProvider serviceProvider, IErrorHandler errorHandler) : ITransactionManager
{
    ITransactionManager<TDbContext> ITransactionManager.GetInstance<TDbContext>() => serviceProvider.GetRequiredService<ITransactionManager<TDbContext>>();

    IErrorHandler ITransactionManager.ErrorHandler => errorHandler;
}

public interface ITransactionManager<TDbContext> where TDbContext : DbContext
{
    ITransactionScope<TDbContext> CreateScope();

    ITransactionScope<TDbContext> Scoped { get; }
}

internal class TransactionManager<TDbContext>(ITransactionScope<TDbContext> defaultScope, ITransactionScopeManager<TDbContext> transactionManager) : ITransactionManager<TDbContext>
    where TDbContext : DbContext
{
    public ITransactionScope<TDbContext> Scoped => defaultScope;

    public ITransactionScope<TDbContext> CreateScope() => transactionManager.CreateScope();
}