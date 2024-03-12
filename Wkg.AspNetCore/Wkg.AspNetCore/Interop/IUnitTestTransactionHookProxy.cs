namespace Wkg.AspNetCore.Interop;

internal interface IUnitTestTransactionHookProxy
{
    internal IUnitTestTransactionHook TransactionHookImplementation { get; }
}
