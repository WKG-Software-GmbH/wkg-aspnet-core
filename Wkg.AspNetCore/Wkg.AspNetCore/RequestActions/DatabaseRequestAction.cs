using Microsoft.EntityFrameworkCore;
using Wkg.AspNetCore.TransactionManagement;

namespace Wkg.AspNetCore.RequestActions;

public delegate ITransactionalContinuation<TResult> DatabaseRequestAction<TDbContext, TResult>(TDbContext dbContext) where TDbContext : DbContext;

public delegate ITransactionalContinuation DatabaseRequestAction<TDbContext>(TDbContext dbContext) where TDbContext : DbContext;