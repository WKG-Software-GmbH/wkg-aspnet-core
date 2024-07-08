using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Wkg.AspNetCore.Abstractions;

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
