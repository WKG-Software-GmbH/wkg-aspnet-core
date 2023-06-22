using Microsoft.EntityFrameworkCore;
using Wkg.AspNetCore.TransactionManagement;

namespace Wkg.AspNetCore.RequestActions;

public delegate Task<ITransactionalContinuation<TResult>> DatabaseRequestTask<TDbContext, TResult>(TDbContext dbContext) where TDbContext : DbContext;

public delegate Task<ITransactionalContinuation> DatabaseRequestTask<TDbContext>(TDbContext dbContext) where TDbContext : DbContext;