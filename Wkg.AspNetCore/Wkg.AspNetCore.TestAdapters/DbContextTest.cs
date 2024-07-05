using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;

namespace Wkg.AspNetCore.TestAdapters;

/// <summary>
/// Represents a test that uses a database context.
/// </summary>
/// <typeparam name="TDbContext">The type of the database context.</typeparam>
/// <typeparam name="TDatabaseLoader">The type of the <see cref="IDatabaseLoader"/> to be used to initialize the database, if necessary.</typeparam>
public abstract class DbContextTest<TDbContext, TDatabaseLoader> : TestBase 
    where TDbContext : DbContext
    where TDatabaseLoader : IDatabaseLoader
{
    /// <inheritdoc/>
    protected override void OnInitialized()
    {
        base.OnInitialized();
        TDatabaseLoader.InitializeDatabase(ServiceProvider);
    }

    /// <summary>
    /// Executes the specified unit test against the database context.
    /// </summary>
    /// <param name="unitTest">The unit test to be executed.</param>
    /// <remarks>
    /// Any changes made to the database context will be rolled back after the unit test has been executed.
    /// </remarks>
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

    /// <summary>
    /// Executes the specified unit test asynchronously against the database context.
    /// </summary>
    /// <param name="unitTestAsync">The unit test to be executed asynchronously.</param>
    /// <remarks>
    /// Any changes made to the database context will be rolled back after the unit test has been executed.
    /// </remarks>
    /// <returns>A task that represents the asynchronous operation.</returns>
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