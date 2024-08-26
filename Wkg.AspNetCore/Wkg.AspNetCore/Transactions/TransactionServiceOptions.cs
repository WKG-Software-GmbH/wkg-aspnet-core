using System.Data;

namespace Wkg.AspNetCore.Transactions;

internal record TransactionServiceOptions(IsolationLevel TransactionIsolationLevel);