using Microsoft.EntityFrameworkCore;

namespace Wkg.AspNetCore.Abstractions.Managers;

internal class ProxiedDatabaseManager<TDbContext>(TDbContext dbContext) : DatabaseManager<TDbContext>(dbContext) where TDbContext : DbContext;