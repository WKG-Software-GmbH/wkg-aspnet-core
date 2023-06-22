using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace Wkg.AspNetCore.TestAdapters;

public abstract class DbContextTest<TDbContext, TDatabaseLoader> : TestBase 
    where TDbContext : DbContext
    where TDatabaseLoader : IDatabaseLoader
{
    private protected override void OnInitialized()
    {
        base.OnInitialized();
        TDatabaseLoader.InitializeDatabase(ServiceProvider);
    }

    /// <summary>
    /// Use this method to perform unit tests on the database context. Any changes made to the database will be rolled back after the unit test completes.
    /// </summary>
    protected void UsingDbContext(Action<TDbContext> unitTest)
    {
        TDbContext dbContext = ServiceProvider.GetRequiredService<TDbContext>();
        using IDbContextTransaction transaction = dbContext.Database.BeginTransaction();
        try
        {
            unitTest.Invoke(dbContext);
        }
        finally
        {
            transaction.Rollback();
        }
    }

    protected async Task UsingDbContextAsync(Func<TDbContext, Task> unitTestAsync)
    {
        TDbContext dbContext = ServiceProvider.GetRequiredService<TDbContext>();
        using IDbContextTransaction transaction = await dbContext.Database.BeginTransactionAsync();
        try
        {
            await unitTestAsync.Invoke(dbContext);
        }
        finally
        {
            await transaction.RollbackAsync();
        }
    }
}