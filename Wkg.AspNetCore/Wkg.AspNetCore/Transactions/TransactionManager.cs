using Microsoft.Extensions.DependencyInjection;
using Wkg.AspNetCore.ErrorHandling;

namespace Wkg.AspNetCore.Transactions;

internal class TransactionManager(IServiceProvider serviceProvider, IErrorSentry errorSentry) : ITransactionManager
{
    ITransactionManager<TDbContext> ITransactionManager.GetInstance<TDbContext>() => serviceProvider.GetRequiredService<ITransactionManager<TDbContext>>();

    IErrorSentry ITransactionManager.ErrorSentry => errorSentry;
}
