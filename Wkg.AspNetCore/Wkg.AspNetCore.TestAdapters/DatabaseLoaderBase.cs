using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace Wkg.AspNetCore.TestAdapters;

public abstract class DatabaseLoaderBase<TDbContext, TDatabaseLoaderImplementation> : IDatabaseLoader 
    where TDbContext : DbContext
    where TDatabaseLoaderImplementation : DatabaseLoaderBase<TDbContext, TDatabaseLoaderImplementation>, new()
{
    protected abstract void InitializeDatabase(TDbContext dbContext);

    static void IDatabaseLoader.InitializeDatabase(ServiceProvider serviceProvider)
    {
        TDatabaseLoaderImplementation loader = new();
        TDbContext dbContext = serviceProvider.GetRequiredService<TDbContext>();
        using IDbContextTransaction transaction = dbContext.Database.BeginTransaction();
        loader.InitializeDatabase(dbContext);
        transaction.Commit();
    }
}
