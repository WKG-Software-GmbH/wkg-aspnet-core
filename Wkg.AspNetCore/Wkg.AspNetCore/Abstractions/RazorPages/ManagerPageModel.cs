using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;
using Wkg.AspNetCore.Abstractions.Controllers;
using Wkg.AspNetCore.Abstractions.Managers;
using Wkg.AspNetCore.Abstractions.Managers.Results;
using Wkg.AspNetCore.Configuration.ManagerBindings;

namespace Wkg.AspNetCore.Abstractions.RazorPages;

/// <summary>
/// Base class for Razor Pages implemented through a manager.
/// </summary>
/// <typeparam name="TManager">The type of the implemented manager.</typeparam>
public abstract class ManagerPageModel<TManager> : PageModel, IMvcContext<TManager>
    where TManager : ManagerBase
{
    private bool _disposedValue;

    /// <summary>
    /// The manager associated with this page.
    /// </summary>
    protected TManager Manager { get; }

    IServiceScope? IMvcContext<TManager>.ServiceScope { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ManagerController{TManager}"/> class.
    /// </summary>
    /// <param name="managerBindings">The manager bindings.</param>
    protected ManagerPageModel(IManagerBindings managerBindings)
    {
        Manager = managerBindings.ActivateManager(this);
    }

    /// <summary>
    /// Handles the specified failed <see cref="ManagerResult"/> by returning the appropriate error response.
    /// </summary>
    /// <param name="result">The failed result to handle.</param>
    /// <returns>The appropriate error response.</returns>
    /// <exception cref="Exception">thrown when the result is an internal server error, delegating the respose to the error handling middleware.</exception>
    /// <exception cref="InvalidOperationException">thrown when the result is successful, as this method should only be called when the result is not successful.</exception>
    /// <exception cref="ArgumentException">thrown when the result code is not a valid result code.</exception>
    protected virtual IActionResult HandleNonSuccess(ManagerResult result) => result.StatusCode switch
    {
        ManagerResultCode.BadRequest => BadRequest(),
        ManagerResultCode.Unauthorized => Unauthorized(),
        ManagerResultCode.Forbidden => Forbid(),
        ManagerResultCode.NotFound => NotFound(),
        ManagerResultCode.InternalServerError => throw new Exception(result.ErrorMessage),
        ManagerResultCode.Success => throw new InvalidOperationException("This method should only be called when the result is not successful."),
        _ => throw new ArgumentException($"{result.StatusCode} is not a valid result code.", nameof(result)),
    };

    /// <summary>
    /// Handles the specified <see cref="ManagerResult"/> by returning the appropriate error response.
    /// </summary>
    /// <param name="result">The result to handle.</param>
    /// <returns>The appropriate API response.</returns>
    /// <exception cref="Exception">thrown when the result is an internal server error, delegating the respose to the error handling middleware.</exception>
    /// <exception cref="ArgumentException">thrown when the result code is not a valid result code.</exception>
    protected virtual IActionResult Handle(ManagerResult result) => result.StatusCode switch
    {
        ManagerResultCode.Success => Page(),
        ManagerResultCode.BadRequest => BadRequest(),
        ManagerResultCode.Unauthorized => Unauthorized(),
        ManagerResultCode.Forbidden => Forbid(),
        ManagerResultCode.NotFound => NotFound(),
        ManagerResultCode.InternalServerError => throw new Exception(result.ErrorMessage),
        _ => throw new ArgumentException($"{result.StatusCode} is not a valid result code.", nameof(result)),
    };

    /// <summary>
    /// Attempts to extract the result from the specified <see cref="ManagerResult{TResult}"/>, returning either the result or the appropriate error response.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="result">The result to extract the value from.</param>
    /// <param name="value">When this method returns, contains the result value if the result was successful; otherwise, <see langword="null"/>.</param>
    /// <param name="error">When this method returns, contains the error response if the result was not successful; otherwise, <see langword="null"/>.</param>
    /// <returns><see langword="true"/> if the result was successful; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="Exception">thrown when the result is an internal server error, delegating the respose to the error handling middleware.</exception>
    /// <exception cref="ArgumentException">thrown when the result code is not a valid result code.</exception>
    protected virtual bool TryExtractResult<TResult>(ManagerResult<TResult> result, [NotNullWhen(true)] out TResult? value, [NotNullWhen(false)] out IActionResult? error)
    {
        if (!result.TryGetResult(out value))
        {
            error = HandleNonSuccess(result);
            return false;
        }
        error = null;
        return true;
    }

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
