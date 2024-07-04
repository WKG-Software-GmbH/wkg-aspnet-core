using Microsoft.Extensions.DependencyInjection;
using Wkg.AspNetCore.ErrorHandling;

namespace Wkg.AspNetCore.Transactions;

internal class TransactionManager(IServiceProvider serviceProvider, IErrorHandler errorHandler) : ITransactionManager
{
    ITransactionManager<TDbContext> ITransactionManager.GetInstance<TDbContext>() => serviceProvider.GetRequiredService<ITransactionManager<TDbContext>>();

    IErrorHandler ITransactionManager.ErrorHandler => errorHandler;
}
