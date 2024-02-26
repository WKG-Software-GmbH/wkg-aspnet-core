namespace Wkg.AspNetCore.Abstractions.Managers;

/// <summary>
/// Provides a base class for all ASP managers.
/// </summary>
public abstract class ManagerBase
{
    /// <summary>
    /// Gets the context of the manager.
    /// </summary>
    internal protected IMvcContext Context { get; internal set; } = null!;
}
