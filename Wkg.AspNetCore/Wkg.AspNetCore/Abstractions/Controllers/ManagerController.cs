using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Wkg.AspNetCore.Abstractions.Managers;
using Wkg.AspNetCore.Configuration.ManagerBindings;

namespace Wkg.AspNetCore.Abstractions.Controllers;

/// <summary>
/// Base class for API controllers implemented through a manager.
/// </summary>
/// <typeparam name="TManager">The type of the implementing manager.</typeparam>
public abstract class ManagerController<TManager> : ControllerBase, IMvcContext<TManager>
    where TManager : ManagerBase
{
    private bool _disposedValue;

    /// <summary>
    /// The manager associated with this controller.
    /// </summary>
    protected TManager Manager { get; }

    IServiceScope? IMvcContext<TManager>.ServiceScope { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ManagerController{TManager}"/> class.
    /// </summary>
    /// <param name="managerBindings">The manager bindings.</param>
    protected ManagerController(IManagerBindings managerBindings)
    {
        Manager = managerBindings.ActivateManager(this);
    }

    IActionResult IMvcContext.Ok() => Ok();

    /// <summary>
    /// Releases the unmanaged resources used by the <see cref="ManagerController{TManager}"/> and optionally releases the managed resources.
    /// </summary>
    protected virtual void Dispose(bool disposing)
    {
        if (disposing && !_disposedValue)
        {
            this.To<IMvcContext<TManager>>().ServiceScope?.Dispose();
            _disposedValue = true;
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
