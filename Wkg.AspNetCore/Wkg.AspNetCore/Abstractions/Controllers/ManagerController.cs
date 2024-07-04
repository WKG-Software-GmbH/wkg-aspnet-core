using Wkg.AspNetCore.Abstractions.Managers;
using Wkg.AspNetCore.Configuration.ManagerBindings;

namespace Wkg.AspNetCore.Abstractions.Controllers;

/// <summary>
/// Base class for API controllers implemented through a manager.
/// </summary>
/// <typeparam name="TManager">The type of the implementing manager.</typeparam>
public abstract class ManagerController<TManager> : WkgControllerBase, IMvcContext<TManager>
    where TManager : ManagerBase
{
    private readonly IManagerBindings _managerBindings;

    /// <summary>
    /// The manager associated with this controller.
    /// </summary>
    protected TManager Manager { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ManagerController{TManager}"/> class.
    /// </summary>
    /// <param name="managerBindings">The manager bindings.</param>
    protected ManagerController(IManagerBindings managerBindings) : base(managerBindings.ErrorHandler)
    {
        Manager = managerBindings.ActivateManager<TManager>(this);
        _managerBindings = managerBindings;
    }

    protected TOtherManager CreateManager<TOtherManager>() where TOtherManager : ManagerBase => 
        _managerBindings.ActivateManager<TOtherManager>(this);
}
