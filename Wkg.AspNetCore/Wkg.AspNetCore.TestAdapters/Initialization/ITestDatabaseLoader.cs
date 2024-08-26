using Microsoft.EntityFrameworkCore;

namespace Wkg.AspNetCore.TestAdapters.Initialization;

/// <summary>
/// Represents setup code that is executed before the first test of the first test class is run.
/// </summary>
public interface ITestDatabaseLoader
{
    internal static abstract void InitializeDatabase(IServiceProvider serviceProvider);
}

/// <inheritdoc />
/// <typeparam name="TDbContext">The type of the database context.</typeparam>
public interface ITestDatabaseLoader<in TDbContext> : ITestDatabaseLoader
    where TDbContext : DbContext
{
    /// <summary>
    /// Initializes the database by inserting the data that is required for the tests.
    /// </summary>
    /// <param name="dbContext">The database context to be used to interact with the database.</param>
    void InitializeDatabase(TDbContext dbContext);
}
