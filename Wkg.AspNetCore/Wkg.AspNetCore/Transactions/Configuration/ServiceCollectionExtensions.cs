using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Data;

namespace Wkg.AspNetCore.Transactions.Configuration;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTransactionManagement<TDbContext>(this IServiceCollection services, Action<TransactionManagerOptionsBuilder>? configureOptions = null) where TDbContext : DbContext
    {
        TransactionManagerOptionsBuilder optionsBuilder = new();
        configureOptions?.Invoke(optionsBuilder);

        TransactionManagerOptions options = new(optionsBuilder.TransactionIsolationLevel);

        services.AddSingleton(options);
        services.TryAddScoped<IDbContextDescriptor, DbContextDescriptor>();
        services.TryAddScoped<ITransactionScopeManager<TDbContext>, TransactionScopeManager<TDbContext>>();
        services.TryAddTransient<ITransactionScope<TDbContext>, TransactionScope<TDbContext>>();
        services.TryAddScoped<ITransactionManager<TDbContext>, TransactionManager<TDbContext>>();
        services.TryAddScoped<ITransactionManager, TransactionManager>();

        return services;
    }
}

public class TransactionManagerOptionsBuilder
{
    internal IsolationLevel TransactionIsolationLevel { get; private set; } = IsolationLevel.ReadCommitted;

    /// <summary>
    /// Sets the default <see cref="IsolationLevel"/> to be used by <see cref="ITransactionManager{TDbContext}"/> instances for all database transactions.
    /// </summary>
    /// <param name="isolationLevel">The <see cref="IsolationLevel"/> to be used.</param>
    /// <returns>The same <see cref="TransactionManagerOptionsBuilder"/> instance for fluent configuration.</returns>
    /// <remarks>
    /// Isolation levels only apply when <see cref="ITransactionManager{TDbContext}"/> instances are used to perform database transactions.
    /// </remarks>
    public TransactionManagerOptionsBuilder UseIsolationLevel(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
    {
        TransactionIsolationLevel = isolationLevel;
        return this;
    }
}