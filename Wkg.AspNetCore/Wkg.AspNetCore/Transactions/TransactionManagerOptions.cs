using System.Data;

namespace Wkg.AspNetCore.Transactions;

internal record TransactionManagerOptions(IsolationLevel TransactionIsolationLevel);