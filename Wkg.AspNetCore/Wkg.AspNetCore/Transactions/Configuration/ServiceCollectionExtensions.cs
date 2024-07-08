using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Wkg.AspNetCore.ErrorHandling;

namespace Wkg.AspNetCore.Transactions.Configuration;

/// <summary>
/// Provides extension methods for configuring transaction management services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds database transaction management services to the specified <see cref="IServiceCollection"/>, 
    /// allowing usage of transactions that may flow across multiple compontents within a shared scope.
    /// </summary>
    /// <typeparam name="TDbContext">The type of the database context to be used for transactions.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
    /// <param name="configureOptions">The optional configuration action for the transaction service options.</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance for fluent configuration.</returns>
    public static IServiceCollection AddTransactionManagement<TDbContext>(this IServiceCollection services, Action<TransactionServiceOptionsBuilder>? configureOptions = null) where TDbContext : DbContext
    {
        TransactionServiceOptionsBuilder optionsBuilder = new();
        configureOptions?.Invoke(optionsBuilder);

        TransactionServiceOptions options = new(optionsBuilder.TransactionIsolationLevel);

        services.AddSingleton(options);
        services.TryAddSingleton<IErrorSentry, DefaultErrorSentry>();
        services.TryAddTransient<ITransaction<TDbContext>, Transaction<TDbContext>>();
        services.TryAddScoped<ITransactionService<TDbContext>, TransactionService<TDbContext>>();
        services.TryAddScoped<ITransactionServiceHandle, TransactionServiceHandle>();

        return services;
    }
}
