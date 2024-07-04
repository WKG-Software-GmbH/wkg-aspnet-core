namespace Wkg.AspNetCore.TransactionManagement;

[Flags]
internal enum TransactionalContinuationType : uint
{
    ReadOnly = 0,
    Commit = 1,
    Rollback = 3,
    ExceptionalRollback = 7
}