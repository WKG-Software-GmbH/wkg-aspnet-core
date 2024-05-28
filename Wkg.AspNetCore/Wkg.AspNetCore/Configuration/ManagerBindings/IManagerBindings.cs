using Wkg.AspNetCore.Abstractions;
using Wkg.AspNetCore.Abstractions.Managers;

namespace Wkg.AspNetCore.Configuration.ManagerBindings;

/// <summary>
/// Represents the bindings of all mapped managers and their DI factories.
/// </summary>
public interface IManagerBindings
{
    internal TManager ActivateManager<TManager>(IMvcContext<TManager> context) where TManager : ManagerBase;

    /// <summary>
    /// Activates the manager associated with the specified context.
    /// </summary>
    /// <remarks>
    /// This method is considered unsafe because nothing prevents the caller from initializing multiple managers bound to the same context.
    /// These managers will not be aware of each other and may cause unexpected behavior.
    /// </remarks>
    /// <typeparam name="TManager">The type of the manager to activate.</typeparam>
    /// <param name="context">The context associated with the manager.</param>
    /// <returns>An instance of the manager associated with the specified context.</returns>
    // TODO: Consider adding a safe version of this method and support sharing state between managers, if necessary (e.g. a DB transaction).
    public TManager ActivateManagerUnsafe<TManager>(IMvcContext context) where TManager : ManagerBase;
}
