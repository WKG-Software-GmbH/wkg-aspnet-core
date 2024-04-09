using Wkg.AspNetCore.Abstractions.Managers.Results;

namespace Wkg.AspNetCore.Abstractions.Managers;

/// <summary>
/// Provides a base class for all ASP managers.
/// </summary>
public abstract class ManagerBase
{
    /// <summary>
    /// Gets the context of the manager.
    /// </summary>
    internal protected IMvcContext Context { get; internal set; } = null!;

    /// <inheritdoc cref="ManagerResult.Success()"/>
    protected static ManagerResult Success() => ManagerResult.Success();

    /// <inheritdoc cref="ManagerResult.NotFound(string?)"/>
    protected static ManagerResult NotFound(string? errorMessage = null) => ManagerResult.NotFound(errorMessage);

    /// <inheritdoc cref="ManagerResult.BadRequest(string?)"/>
    protected static ManagerResult BadRequest(string? errorMessage = null) => ManagerResult.BadRequest(errorMessage);

    /// <inheritdoc cref="ManagerResult.Unauthorized(string?)"/>
    protected static ManagerResult Unauthorized(string? errorMessage = null) => ManagerResult.Unauthorized(errorMessage);

    /// <inheritdoc cref="ManagerResult.Forbidden(string?)"/>
    protected static ManagerResult Forbidden(string? errorMessage = null) => ManagerResult.Forbidden(errorMessage);

    /// <inheritdoc cref="ManagerResult.InternalServerError(string?)"/>
    protected static ManagerResult InternalServerError(string errorMessage) => ManagerResult.InternalServerError(errorMessage);

    /// <inheritdoc cref="ManagerResult.Success{TResult}(TResult)"/>
    protected static ManagerResult<TResult> Success<TResult>(TResult result) => ManagerResult.Success(result);

    /// <inheritdoc cref="ManagerResult.NotFound{TResult}(string?)"/>
    protected static ManagerResult<TResult> NotFound<TResult>(string? errorMessage = null) => ManagerResult.NotFound<TResult>(errorMessage);

    /// <inheritdoc cref="ManagerResult.BadRequest{TResult}(string?)"/>
    protected static ManagerResult<TResult> BadRequest<TResult>(string? errorMessage = null) => ManagerResult.BadRequest<TResult>(errorMessage);

    /// <inheritdoc cref="ManagerResult.Unauthorized{TResult}(string?)"/>
    protected static ManagerResult<TResult> Unauthorized<TResult>(string? errorMessage = null) => ManagerResult.Unauthorized<TResult>(errorMessage);

    /// <inheritdoc cref="ManagerResult.Forbidden{TResult}(string?)"/>
    protected static ManagerResult<TResult> Forbidden<TResult>(string? errorMessage = null) => ManagerResult.Forbidden<TResult>(errorMessage);

    /// <inheritdoc cref="ManagerResult.InternalServerError{TResult}(string?)"/>
    protected static ManagerResult<TResult> InternalServerError<TResult>(string errorMessage) => ManagerResult.InternalServerError<TResult>(errorMessage);
}
