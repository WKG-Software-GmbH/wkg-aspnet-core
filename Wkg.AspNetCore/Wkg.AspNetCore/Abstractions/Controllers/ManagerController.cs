using Microsoft.AspNetCore.Mvc;
using Wkg.AspNetCore.Abstractions.Managers;
using Wkg.AspNetCore.Configuration.ManagerBindings;

namespace Wkg.AspNetCore.Abstractions.Controllers;

/// <summary>
/// Base class for API controllers proxying a manager.
/// </summary>
/// <typeparam name="TManager">The type of the manager to proxy.</typeparam>
public abstract class ManagerController<TManager> : ControllerBase, IMvcContext<TManager>
    where TManager : ManagerBase
{
    /// <summary>
    /// The manager associated with this controller.
    /// </summary>
    protected TManager Manager { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ManagerController{TManager}"/> class.
    /// </summary>
    /// <param name="managerBindings">The manager bindings.</param>
    protected ManagerController(IManagerBindings managerBindings)
    {
        Manager = managerBindings.ActivateManager<TManager>(this);
    }

    IActionResult IMvcContext.Ok() => Ok();
}
