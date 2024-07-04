using Microsoft.EntityFrameworkCore;

namespace Wkg.AspNetCore.Transactions;

internal class TransactionManager<TDbContext>(ITransactionScope<TDbContext> defaultScope, ITransactionScopeManager<TDbContext> transactionManager) : ITransactionManager<TDbContext>
    where TDbContext : DbContext
{
    public ITransactionScope<TDbContext> Scoped => defaultScope;

    public ITransactionScope<TDbContext> CreateScope() => transactionManager.CreateScope();
}