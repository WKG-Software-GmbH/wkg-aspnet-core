using Microsoft.EntityFrameworkCore;
using Wkg.AspNetCore.ErrorHandling;

namespace Wkg.AspNetCore.Transactions;

/// <summary>
/// Represents a handle to the transaction service of this scope.
/// </summary>
public interface ITransactionServiceHandle
{
    internal ITransactionService<TDbContext> GetInstance<TDbContext>() where TDbContext : DbContext;

    internal IErrorSentry ErrorSentry { get; }
}