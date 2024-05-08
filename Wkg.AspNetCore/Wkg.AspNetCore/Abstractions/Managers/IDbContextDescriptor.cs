using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Wkg.AspNetCore.Abstractions.Managers;

/// <summary>
/// Represents a service descriptor for a database context.
/// </summary>
public interface IDbContextDescriptor
{
    internal TDbContext GetDbContext<TDbContext>() where TDbContext : DbContext;
}

internal class DbContextDescriptor(IServiceProvider serviceProvider) : IDbContextDescriptor
{
    TDbContext IDbContextDescriptor.GetDbContext<TDbContext>() => serviceProvider.GetRequiredService<TDbContext>();
}