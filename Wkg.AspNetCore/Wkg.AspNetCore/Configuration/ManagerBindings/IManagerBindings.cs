using Wkg.AspNetCore.Abstractions;
using Wkg.AspNetCore.Abstractions.Managers;

namespace Wkg.AspNetCore.Configuration.ManagerBindings;

/// <summary>
/// Represents the bindings of all mapped managers and their DI factories.
/// </summary>
public interface IManagerBindings
{
    internal TManager ActivateManager<TManager>(IMvcContext<TManager> context) where TManager : ManagerBase;
}
