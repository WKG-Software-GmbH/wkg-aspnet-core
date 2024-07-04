using Wkg.AspNetCore.Abstractions;
using Wkg.AspNetCore.Abstractions.Managers;
using Wkg.AspNetCore.ErrorHandling;

namespace Wkg.AspNetCore.Configuration.ManagerBindings;

/// <summary>
/// Represents the bindings of all mapped managers and their DI factories.
/// </summary>
public interface IManagerBindings
{
    /// <summary>
    /// Activates the manager associated with the specified context.
    /// </summary>
    /// <typeparam name="TManager">The type of the manager to activate.</typeparam>
    /// <param name="context">The context associated with the manager.</param>
    /// <returns>An instance of the manager associated with the specified context.</returns>
    public TManager ActivateManager<TManager>(IMvcContext context) where TManager : ManagerBase;

    internal IErrorHandler ErrorHandler { get; }
}
