using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Wkg.AspNetCore.TestAdapters.Initialization;

namespace Wkg.AspNetCore.TestAdapters;

/// <summary>
/// Represents a test that uses a database context.
/// </summary>
/// <typeparam name="TDbContext">The type of the database context.</typeparam>
/// <typeparam name="TInitializer">The <see cref="IDITestInitializer"/> implementation to be used for test initialization.</typeparam>
public abstract class DbContextTest<TDbContext, TInitializer> : DIAwareTestBase<TInitializer>
    where TDbContext : DbContext
    where TInitializer : IDITestInitializer
{
    /// <summary>
    /// Executes the specified unit test against the database context.
    /// </summary>
    /// <param name="unitTestAction">The unit test to be executed.</param>
    /// <remarks>
    /// Any changes made to the database context will be rolled back after the unit test has been executed.
    /// </remarks>
    protected Task UsingDbContextAsync(Action<TDbContext> unitTestAction) => UsingServiceProviderAsync(async serviceProvider =>
    {
        TDbContext dbContext = serviceProvider.GetRequiredService<TDbContext>();
        await using IDbContextTransaction transaction = await dbContext.Database.BeginTransactionAsync();
        try
        {
            unitTestAction.Invoke(dbContext);
        }
        finally
        {
            await transaction.RollbackAsync();
        }
    });

    /// <summary>
    /// Executes the specified unit test asynchronously against the database context.
    /// </summary>
    /// <param name="unitTestTask">The unit test to be executed asynchronously.</param>
    /// <remarks>
    /// Any changes made to the database context will be rolled back after the unit test has been executed.
    /// </remarks>
    /// <returns>A task that represents the asynchronous operation.</returns>
    protected Task UsingDbContextAsync(Func<TDbContext, Task> unitTestTask) => UsingServiceProviderAsync(async serviceProvider =>
    {
        TDbContext dbContext = serviceProvider.GetRequiredService<TDbContext>();
        await using IDbContextTransaction transaction = await dbContext.Database.BeginTransactionAsync();
        try
        {
            await unitTestTask.Invoke(dbContext);
        }
        finally
        {
            await transaction.RollbackAsync();
        }
    });
}