using Microsoft.EntityFrameworkCore;
using Wkg.AspNetCore.ErrorHandling;

namespace Wkg.AspNetCore.Transactions;

public interface ITransactionManager
{
    internal ITransactionManager<TDbContext> GetInstance<TDbContext>() where TDbContext : DbContext;

    internal IErrorHandler ErrorHandler { get; }
}

public interface ITransactionManager<TDbContext> where TDbContext : DbContext
{
    ITransactionScope<TDbContext> CreateScope();

    ITransactionScope<TDbContext> Scoped { get; }
}
