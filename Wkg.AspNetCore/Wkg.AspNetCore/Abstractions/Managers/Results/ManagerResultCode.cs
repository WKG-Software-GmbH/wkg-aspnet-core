using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Wkg.AspNetCore.Abstractions.Managers.Results;

/// <summary>
/// Represents the result code of a manager operation.
/// </summary>
public enum ManagerResultCode : uint
{
    /// <summary>
    /// Represents an unknown result code. In most contexts, this value is invalid.
    /// </summary>
    Unknown = default,

    /// <summary>
    /// The operation was successful.
    /// </summary>
    Success = 200,

    /// <summary>
    /// The request was malformed or invalid.
    /// </summary>
    /// <remarks>
    /// An unhandlable error associated with the request itself, usually resulting in a hard HTTP 400 response to the client.
    /// </remarks>
    BadRequest = 400,

    /// <summary>
    /// The request was unauthorized.
    /// </summary>
    Unauthorized = 401,

    /// <summary>
    /// Access to the resource is forbidden.
    /// </summary>
    Forbidden = 403,

    /// <summary>
    /// The requested resource was not found.
    /// </summary>
    NotFound = 404,

    /// <summary>
    /// The request was invalid due to the state of the model.
    /// Indicates that the <see cref="ModelStateDictionary"/> associated with the request contains errors.
    /// </summary>
    /// <remarks>
    /// A "soft" version of <see cref="BadRequest"/> that is used when the issue was handled successfully and returned as a model state error.
    /// Usually still allows for a successful response to be returned to the client.
    /// </remarks>
    InvalidModelState = 422,

    /// <summary>
    /// An internal server error occurred.
    /// </summary>
    InternalServerError = 500
}