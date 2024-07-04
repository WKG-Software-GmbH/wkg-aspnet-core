using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Wkg.AspNetCore.Abstractions.Managers;

namespace Wkg.AspNetCore.Abstractions;

public interface IMvcContext<TManager> : IMvcContext where TManager : ManagerBase;

/// <summary>
/// Represents the context of an MVC request.
/// </summary>
public interface IMvcContext
{
    /// <summary>
    /// Gets the model state of the request.
    /// </summary>
    ModelStateDictionary ModelState { get; }

    /// <summary>
    /// The <see cref="Microsoft.AspNetCore.Http.HttpContext"/> associated with the current request.
    /// </summary>
    HttpContext HttpContext { get; }
}
