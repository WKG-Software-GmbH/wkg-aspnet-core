using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Wkg.AspNetCore.Transactions;

namespace Wkg.AspNetCore.TestAdapters.Extensions;

/// <summary>
/// Extension methods for <see cref="IServiceCollection"/>.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Mocks all <see cref="ITransaction{TDbContext}"/> instances executed against the <typeparamref name="TDbContext"/> database 
    /// context by replacing commits with rollbacks to prevent changes made during tests from persisting.
    /// </summary>
    /// <typeparam name="TDbContext">The type of the database context to be used for transactions.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance for fluent configuration.</returns>
    public static IServiceCollection MockDatabaseTransactions<TDbContext>(this IServiceCollection services) where TDbContext : DbContext
    {
        services.AddTransient<ITransaction<TDbContext>, DummyTransaction<TDbContext>>();
        return services;
    }
}
