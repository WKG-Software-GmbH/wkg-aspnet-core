using System.Diagnostics;

namespace Wkg.AspNetCore.TransactionManagement;

public interface ITransactionalContinuation
{
    internal TransactionalContinuationType NextAction { get; }
}
