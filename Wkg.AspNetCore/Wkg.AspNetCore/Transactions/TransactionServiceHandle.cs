using Microsoft.Extensions.DependencyInjection;
using Wkg.AspNetCore.ErrorHandling;

namespace Wkg.AspNetCore.Transactions;

internal class TransactionServiceHandle(IServiceProvider serviceProvider, IErrorSentry errorSentry) : ITransactionServiceHandle
{
    ITransactionService<TDbContext> ITransactionServiceHandle.GetInstance<TDbContext>() => serviceProvider.GetRequiredService<ITransactionService<TDbContext>>();

    IErrorSentry ITransactionServiceHandle.ErrorSentry => errorSentry;
}
