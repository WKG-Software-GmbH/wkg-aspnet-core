using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace Wkg.AspNetCore.TestAdapters;

/// <summary>
/// Provides a base class for <see cref="IDatabaseLoader"/> implementations.
/// </summary>
/// <typeparam name="TDbContext">The type of the database context.</typeparam>
/// <typeparam name="TDatabaseLoaderImplementation">The type of the implementing database loader.</typeparam>
public abstract class DatabaseLoaderBase<TDbContext, TDatabaseLoaderImplementation> : IDatabaseLoader 
    where TDbContext : DbContext
    where TDatabaseLoaderImplementation : DatabaseLoaderBase<TDbContext, TDatabaseLoaderImplementation>, new()
{
    /// <summary>
    /// Initializes the database by inserting the data that is required for the tests.
    /// </summary>
    /// <param name="dbContext">The database context to be used to interact with the database.</param>
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
