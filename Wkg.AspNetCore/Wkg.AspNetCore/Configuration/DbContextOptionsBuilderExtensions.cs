using Microsoft.EntityFrameworkCore;
using System.Data;
using Wkg.AspNetCore.Abstractions.Managers;

namespace Wkg.AspNetCore.Configuration;

/// <summary>
/// Provides extension methods for <see cref="DbContextOptionsBuilder"/>.
/// </summary>
public static class DbContextOptionsBuilderExtensions
{
    /// <summary>
    /// Sets the default <see cref="IsolationLevel"/> to be used by <see cref="DatabaseManager{TDbContext}"/> instances for all database transactions.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <param name="isolationLevel">The <see cref="IsolationLevel"/> to be used.</param>
    /// <returns>The same <see cref="DbContextOptionsBuilder"/> instance for fluent configuration.</returns>
    /// <remarks>
    /// Isolation levels only apply when <see cref="DatabaseManager{TDbContext}"/> is used to perform database transactions and can 
    /// be overridden by the <see cref="DatabaseManager{TDbContext}"/> instance.
    /// </remarks>
    public static DbContextOptionsBuilder UseDefaultIsolationLevel(this DbContextOptionsBuilder builder, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
    {
        DatabaseTransactionDefaults.DefaultIsolationLevel = isolationLevel;
        return builder;
    }
}
