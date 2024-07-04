using Wkg.AspNetCore.Abstractions.Controllers;
using Wkg.AspNetCore.Abstractions.Managers;
using Wkg.AspNetCore.Configuration.ManagerBindings;

namespace Wkg.AspNetCore.Abstractions.RazorPages;

/// <summary>
/// Base class for Razor Pages implemented through a manager.
/// </summary>
/// <typeparam name="TManager">The type of the implemented manager.</typeparam>
public abstract class ManagerPageModel<TManager> : WkgPageModel, IMvcContext<TManager>
    where TManager : ManagerBase
{
    private readonly IManagerBindings _managerBindings;

    /// <summary>
    /// The manager associated with this page.
    /// </summary>
    protected TManager Manager { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ManagerController{TManager}"/> class.
    /// </summary>
    /// <param name="managerBindings">The manager bindings.</param>
    protected ManagerPageModel(IManagerBindings managerBindings) : base(managerBindings.ErrorHandler)
    {
        Manager = managerBindings.ActivateManager<TManager>(this);
        _managerBindings = managerBindings;
    }

    protected TOtherManager CreateManager<TOtherManager>() where TOtherManager : ManagerBase =>
        _managerBindings.ActivateManager<TOtherManager>(this);
}
