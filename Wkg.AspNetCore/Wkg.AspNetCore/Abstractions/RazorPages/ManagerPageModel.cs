using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wkg.AspNetCore.Abstractions.Controllers;
using Wkg.AspNetCore.Abstractions.Managers;
using Wkg.AspNetCore.Configuration.ManagerBindings;

namespace Wkg.AspNetCore.Abstractions.RazorPages;

/// <summary>
/// Base class for Razor Pages proxying a manager.
/// </summary>
/// <typeparam name="TManager">The type of the manager to proxy.</typeparam>
public abstract class ManagerPageModel<TManager> : PageModel, IMvcContext<TManager>
    where TManager : ManagerBase
{
    /// <summary>
    /// The manager associated with this page.
    /// </summary>
    protected TManager Manager { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ManagerController{TManager}"/> class.
    /// </summary>
    /// <param name="managerBindings">The manager bindings.</param>
    protected ManagerPageModel(IManagerBindings managerBindings)
    {
        Manager = managerBindings.ActivateManager<TManager>(this);
    }

    IActionResult IMvcContext.Ok() => Page();
}
