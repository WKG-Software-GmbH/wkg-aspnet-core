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
    /// An internal server error occurred.
    /// </summary>
    InternalServerError = 500
}