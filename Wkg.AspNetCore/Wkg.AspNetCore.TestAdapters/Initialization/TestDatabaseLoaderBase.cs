using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace Wkg.AspNetCore.TestAdapters.Initialization;

/// <summary>
/// Provides a base class for <see cref="ITestDatabaseLoader"/> implementations.
/// </summary>
/// <typeparam name="TSelf">The type of the implementing database loader.</typeparam>
/// <typeparam name="TDbContext">The type of the database context.</typeparam>
public abstract class TestDatabaseLoaderBase<TSelf, TDbContext> : ITestDatabaseLoader
    where TSelf : TestDatabaseLoaderBase<TSelf, TDbContext>, ITestDatabaseLoader<TDbContext>, new()
    where TDbContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="TestDatabaseLoaderBase{TSelf, TDbContext}"/> class.
    /// </summary>
    protected TestDatabaseLoaderBase()
    {
        if (this is not TSelf)
        {
            throw new InvalidOperationException($"The type {typeof(TSelf).Name} cannot be used as type parameter {nameof(TSelf)} in the generic type {nameof(TestDatabaseLoaderBase<TSelf, TDbContext>)} for derived type {GetType().Name}. " +
                $"There is no type parameter conversion from {GetType().Name} to {typeof(TSelf).Name}.");
        }
    }

    static void ITestDatabaseLoader.InitializeDatabase(IServiceProvider serviceProvider)
    {
        TSelf databaseLoader = new();
        TDbContext dbContext = serviceProvider.GetRequiredService<TDbContext>();
        using IDbContextTransaction transaction = dbContext.Database.BeginTransaction();
        databaseLoader.InitializeDatabase(dbContext);
        transaction.Commit();
    }
}