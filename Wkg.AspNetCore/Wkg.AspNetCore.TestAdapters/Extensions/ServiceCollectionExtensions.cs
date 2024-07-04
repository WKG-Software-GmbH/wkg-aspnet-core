using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Wkg.AspNetCore.Transactions;

namespace Wkg.AspNetCore.TestAdapters.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection MockDatabaseTransactions<TDbContext>(this IServiceCollection services) where TDbContext : DbContext
    {
        services.AddTransient<ITransactionScope<TDbContext>, DummyTransactionScope<TDbContext>>();
        return services;
    }
}
