namespace Wkg.AspNetCore.Interop;

/// <summary>
/// Represents a hook for unit tests to control the transaction management of ASP.NET Core API controllers.
/// </summary>
internal interface IUnitTestTransactionHook
{
    /// <summary>
    /// Gets or sets a value indicating whether the API controller should yield control over the transaction management to the unit test runner.
    /// </summary>
    internal bool ExternalTransactionManagement__UNIT_TEST_HOOK { get; set; }

    /// <summary>
    /// Rolls back the currently active transaction.
    /// </summary>
    internal void RollbackTransaction__UNIT_TEST_HOOK();

    /// <summary>
    /// Rolls back the currently active transaction.
    /// </summary>
    internal ValueTask RollbackTransactionAsync__UNIT_TEST_HOOK();
}
