using Microsoft.EntityFrameworkCore;

namespace Wkg.AspNetCore.Abstractions.Managers;

internal class ProxiedDatabaseManager<TDbContext>(TDbContext dbContext, bool autoAssertModelState) : DatabaseManager<TDbContext>(dbContext, autoAssertModelState) where TDbContext : DbContext;