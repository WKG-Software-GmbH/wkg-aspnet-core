using Microsoft.EntityFrameworkCore;

namespace Wkg.AspNetCore.Abstractions.Managers;

internal class ProxiedDatabaseManager<TDbContext>(IDbContextDescriptor dbContextDescriptor) : DatabaseManager<TDbContext>(dbContextDescriptor) where TDbContext : DbContext;