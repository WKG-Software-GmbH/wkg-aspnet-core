using Microsoft.EntityFrameworkCore;
using System.Data;

namespace Wkg.AspNetCore.Configuration;

/// <summary>
/// Provides extension methods for <see cref="DbContextOptionsBuilder"/>.
/// </summary>
public static class DbContextOptionsBuilderExtensions
{
    /// <summary>
    /// Sets the default <see cref="IsolationLevel"/> to be used for all database transactions.
    /// </summary>
    /// <param name="builder">The builder.</param>
    /// <param name="isolationLevel">The <see cref="IsolationLevel"/> to be used.</param>
    /// <returns>The same <see cref="DbContextOptionsBuilder"/> instance for fluent configuration.</returns>
    public static DbContextOptionsBuilder UseDefaultIsolationLevel(this DbContextOptionsBuilder builder, IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
    {
        DatabaseControllerDefaults.DefaultIsolationLevel = isolationLevel;
        return builder;
    }
}
