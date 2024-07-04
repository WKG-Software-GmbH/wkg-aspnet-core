using Microsoft.EntityFrameworkCore;

namespace Wkg.AspNetCore.Transactions;

public interface ITransactionManager<TDbContext> where TDbContext : DbContext
{
    ITransactionScope<TDbContext> CreateScope();

    ITransactionScope<TDbContext> Scoped { get; }
}
