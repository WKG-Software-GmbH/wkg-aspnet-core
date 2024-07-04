using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Wkg.AspNetCore.ErrorHandling;
using Wkg.AspNetCore.Transactions.Internals;

namespace Wkg.AspNetCore.Transactions.Configuration;

/// <summary>
/// Provides extension methods for configuring transaction management services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds database transaction management services to the specified <see cref="IServiceCollection"/>, 
    /// allowing usage of scoped transactions that may flow across multiple compontents within a shared scope.
    /// </summary>
    /// <typeparam name="TDbContext">The type of the database context to be used for transactions.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
    /// <param name="configureOptions">The optional configuration action for the transaction manager options.</param>
    /// <returns>The same <see cref="IServiceCollection"/> instance for fluent configuration.</returns>
    public static IServiceCollection AddTransactionManagement<TDbContext>(this IServiceCollection services, Action<TransactionManagerOptionsBuilder>? configureOptions = null) where TDbContext : DbContext
    {
        TransactionManagerOptionsBuilder optionsBuilder = new();
        configureOptions?.Invoke(optionsBuilder);

        TransactionManagerOptions options = new(optionsBuilder.TransactionIsolationLevel);

        services.AddSingleton(options);
        services.TryAddSingleton<IErrorSentry, DefaultErrorSentry>();
        services.TryAddScoped<IDbContextDescriptor, DbContextDescriptor>();
        services.TryAddScoped<ITransactionScopeManager<TDbContext>, TransactionScopeManager<TDbContext>>();
        services.TryAddTransient<ITransactionScope<TDbContext>, TransactionScope<TDbContext>>();
        services.TryAddScoped<ITransactionManager<TDbContext>, TransactionManager<TDbContext>>();
        services.TryAddScoped<ITransactionManager, TransactionManager>();

        return services;
    }
}
