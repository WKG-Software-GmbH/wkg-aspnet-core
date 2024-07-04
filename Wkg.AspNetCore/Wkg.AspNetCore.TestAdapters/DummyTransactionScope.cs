using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Wkg.AspNetCore.ErrorHandling;
using Wkg.AspNetCore.Transactions;
using Wkg.AspNetCore.Transactions.Internals;

namespace Wkg.AspNetCore.TestAdapters;

internal class DummyTransactionScope<TDbContext>(ITransactionScopeManager<TDbContext> transactionManager, IErrorSentry errorHandler) 
    : TransactionScope<TDbContext>(transactionManager, errorHandler) where TDbContext : DbContext
{
    // prevent the transaction from being committed
    internal override Task CommitAsync(IDbContextTransaction transaction) => transaction.RollbackAsync();
}
