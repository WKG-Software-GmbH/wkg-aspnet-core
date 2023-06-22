namespace Wkg.AspNetCore.TransactionManagement;

internal enum TransactionalContinuationType : uint
{
    Commit = 0,
    Rollback = ~Commit
}