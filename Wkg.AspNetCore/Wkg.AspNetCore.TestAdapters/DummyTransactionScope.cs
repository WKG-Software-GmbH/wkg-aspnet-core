using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Wkg.AspNetCore.ErrorHandling;
using Wkg.AspNetCore.Transactions;

namespace Wkg.AspNetCore.TestAdapters;

internal class DummyTransactionScope<TDbContext>(ITransactionScopeManager<TDbContext> transactionManager, IErrorHandler errorHandler) 
    : TransactionScope<TDbContext>(transactionManager, errorHandler) where TDbContext : DbContext
{
    // prevent the transaction from being committed
    internal override Task CommitAsync(IDbContextTransaction transaction) => transaction.RollbackAsync();
}
