using Microsoft.EntityFrameworkCore;
using System.Data;

namespace Wkg.AspNetCore.Transactions;

/// <summary>
/// Represents a service that provides an abstraction for managing and automating database transactions in a unit test-friendly way.
/// </summary>
/// <typeparam name="TDbContext">The type of the database context.</typeparam>
public interface ITransactionService<TDbContext> where TDbContext : DbContext
{
    /// <summary>
    /// Retrieves an <see cref="ITransaction{TDbContext}"/> with its lifetime bound to the current DI scope.
    /// </summary>
    /// <remarks>
    /// In ASP.NET Core applications, scoped transactions can be used to ensure that database operations within a single HTTP request are performed within the same transaction, 
    /// even if they are executed by different components. As a result, a scoped transaction automatically flows across all components that are executed within the same HTTP request,
    /// keeping database access isolated and consistent.
    /// <para>
    /// The <see cref="System.Data.IsolationLevel"/> of scoped transactions is set to the value specified during service registration,
    /// or to the default value of <see cref="IsolationLevel.ReadCommitted"/> if no value was specified.
    /// </para>
    /// </remarks>
    ITransaction<TDbContext> Scoped { get; }

    /// <summary>
    /// Gets or sets the default <see cref="System.Data.IsolationLevel"/> to be used by <see cref="ITransaction{TDbContext}"/> instances that are created manually by this service.
    /// </summary>
    IsolationLevel IsolationLevel { get; set; }

    /// <summary>
    /// Begins a new transaction with the specified <see cref="System.Data.IsolationLevel"/> in a new DI scope. In normal ASP.NET Core scenarios, this method should not be used directly.
    /// </summary>
    /// <remarks>
    /// As a general rule, it is recommended to use the <see cref="Scoped"/> property to retrieve a transaction instance with a lifetime bound to the current DI scope.
    /// <para>
    /// Remember to always dispose of the transaction instance after use to ensure that the created DI scope is properly cleaned up.
    /// </para>
    /// </remarks>
    /// <param name="isolationLevel">The <see cref="System.Data.IsolationLevel"/> to be used for the transaction.</param>
    /// <returns>An <see cref="ITransaction{TDbContext}"/> instance representing the new transaction.</returns>
    ITransaction<TDbContext> BeginTransaction(IsolationLevel? isolationLevel = default);
}
