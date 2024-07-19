using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Wkg.AspNetCore.ErrorHandling;
using Wkg.AspNetCore.Transactions;

namespace Wkg.AspNetCore.TestAdapters.Initialization;

internal class MockedTransaction<TDbContext>(TDbContext dbContext, IErrorSentry errorSentry, TransactionServiceOptions options)
    : Transaction<TDbContext>(dbContext, errorSentry, options) where TDbContext : DbContext
{
    // prevent the transaction from being committed
    internal override Task CommitAsync(IDbContextTransaction transaction) => transaction.RollbackAsync();
}
