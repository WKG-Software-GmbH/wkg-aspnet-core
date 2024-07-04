using System.Runtime.CompilerServices;
using Wkg.AspNetCore.Abstractions.Managers.Results;
using Wkg.AspNetCore.Configuration.ManagerBindings;
using Wkg.AspNetCore.ErrorHandling;

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

    internal IManagerBindings Bindings { get; set; } = null!;

    /// <summary>
    /// Gets the <see cref="IErrorSentry"/> associated with this context.
    /// </summary>
    protected IErrorSentry ErrorSentry => Bindings.ErrorSentry;

    /// <summary>
    /// Creates a new manager of the specified type, flowing the current context (e.g. user, request, database transaction) as needed.
    /// </summary>
    /// <typeparam name="TManager">The type of the manager to create.</typeparam>
    /// <returns>An instance of the manager of the specified type.</returns>
    protected TManager CreateManager<TManager>() where TManager : ManagerBase => Bindings.ActivateManager<TManager>(Context);

    /// <inheritdoc cref="ManagerResult.Success()"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected static ManagerResult Success() => ManagerResult.Success();

    /// <inheritdoc cref="ManagerResult.NotFound(string?)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected static ManagerResult NotFound(string? errorMessage = null) => ManagerResult.NotFound(errorMessage);

    /// <inheritdoc cref="ManagerResult.BadRequest(string?)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected static ManagerResult BadRequest(string? errorMessage = null) => ManagerResult.BadRequest(errorMessage);

    /// <inheritdoc cref="ManagerResult.Unauthorized(string?)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected static ManagerResult Unauthorized(string? errorMessage = null) => ManagerResult.Unauthorized(errorMessage);

    /// <inheritdoc cref="ManagerResult.Forbidden(string?)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected static ManagerResult Forbidden(string? errorMessage = null) => ManagerResult.Forbidden(errorMessage);

    /// <inheritdoc cref="ManagerResult.InvalidModelState()"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected static ManagerResult InvalidModelState() => ManagerResult.InvalidModelState();

    /// <inheritdoc cref="ManagerResult.InternalServerError(string?)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected static ManagerResult InternalServerError(string errorMessage) => ManagerResult.InternalServerError(errorMessage);

    /// <inheritdoc cref="ManagerResult.Success{TResult}(TResult)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected static ManagerResult<TResult> Success<TResult>(TResult result) => ManagerResult.Success(result);

    /// <inheritdoc cref="ManagerResult.NotFound{TResult}(string?)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected static ManagerResult<TResult> NotFound<TResult>(string? errorMessage = null) => ManagerResult.NotFound<TResult>(errorMessage);

    /// <inheritdoc cref="ManagerResult.BadRequest{TResult}(string?)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected static ManagerResult<TResult> BadRequest<TResult>(string? errorMessage = null) => ManagerResult.BadRequest<TResult>(errorMessage);

    /// <inheritdoc cref="ManagerResult.Unauthorized{TResult}(string?)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected static ManagerResult<TResult> Unauthorized<TResult>(string? errorMessage = null) => ManagerResult.Unauthorized<TResult>(errorMessage);

    /// <inheritdoc cref="ManagerResult.Forbidden{TResult}(string?)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected static ManagerResult<TResult> Forbidden<TResult>(string? errorMessage = null) => ManagerResult.Forbidden<TResult>(errorMessage);

    /// <inheritdoc cref="ManagerResult.InvalidModelState{TResult}()"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected static ManagerResult<TResult> InvalidModelState<TResult>() => ManagerResult.InvalidModelState<TResult>();

    /// <inheritdoc cref="ManagerResult.InternalServerError{TResult}(string?)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected static ManagerResult<TResult> InternalServerError<TResult>(string errorMessage) => ManagerResult.InternalServerError<TResult>(errorMessage);
}