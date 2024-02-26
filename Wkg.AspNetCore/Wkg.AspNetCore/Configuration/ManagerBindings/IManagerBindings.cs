using Wkg.AspNetCore.Abstractions;
using Wkg.AspNetCore.Abstractions.Managers;

namespace Wkg.AspNetCore.Configuration.ManagerBindings;

/// <summary>
/// Represents a binding between a controller and a manager.
/// </summary>
public interface IManagerBindings
{
    internal TManager ActivateManager<TManager>(IMvcContext context) where TManager : ManagerBase;
}
