using Microsoft.EntityFrameworkCore;
using Wkg.AspNetCore.ErrorHandling;

namespace Wkg.AspNetCore.Transactions;

public interface ITransactionManager
{
    internal ITransactionManager<TDbContext> GetInstance<TDbContext>() where TDbContext : DbContext;

    internal IErrorSentry ErrorSentry { get; }
}