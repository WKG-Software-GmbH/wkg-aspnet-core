using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.DependencyInjection;
using Wkg.AspNetCore.Abstractions.Managers;

namespace Wkg.AspNetCore.Abstractions;

internal interface IMvcContext<TManager> : IMvcContext, IDisposable where TManager : ManagerBase
{
    internal IServiceScope? ServiceScope { get; set; }
}

/// <summary>
/// Represents the context of an MVC request.
/// </summary>
public interface IMvcContext
{
    /// <summary>
    /// Gets the HTTP context of the request.
    /// </summary>
    HttpContext HttpContext { get; }

    /// <summary>
    /// Gets the model state of the request.
    /// </summary>
    ModelStateDictionary ModelState { get; }
}
