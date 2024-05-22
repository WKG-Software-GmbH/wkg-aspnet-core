using Microsoft.AspNetCore.Mvc;
using Wkg.AspNetCore.RequestActions;
using System.Runtime.CompilerServices;
using Wkg.AspNetCore.Abstractions.Managers;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Wkg.AspNetCore.Abstractions.Controllers;
using Wkg.AspNetCore.Exceptions;

namespace Wkg.AspNetCore.Abstractions.RazorPages;

/// <summary>
/// Provides a base class for API controllers to handle exceptions.
/// </summary>
public abstract class ErrorHandlingPageModel : PageModel, IMvcContext
{
    private readonly ErrorHandlingManager _implementation;

    /// <summary>
    /// Initializes a new instance of the <see cref="ErrorHandlingController"/> class.
    /// </summary>
    protected ErrorHandlingPageModel()
    {
        _implementation = new ProxiedErrorHandlingManager()
        {
            Context = this
        };
    }

    /// <inheritdoc cref="ErrorHandlingManager.WithErrorHandling(RequestAction{IActionResult})"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected virtual IActionResult WithErrorHandling(RequestAction<IActionResult> action) =>
        _implementation.WithErrorHandling(action);

    /// <inheritdoc cref="ErrorHandlingManager.WithErrorHandling(RequestAction)"/>/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected virtual void WithErrorHandling(RequestAction action) =>
        _implementation.WithErrorHandling(action);

    /// <inheritdoc cref="ErrorHandlingManager.WithErrorHandling{TResult}(RequestAction{TResult})"/>/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected virtual TResult WithErrorHandling<TResult>(RequestAction<TResult> action) =>
        _implementation.WithErrorHandling(action);

    /// <inheritdoc cref="ErrorHandlingManager.WithErrorHandlingAsync(RequestTask{IActionResult})"/>/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected virtual Task<IActionResult> WithErrorHandlingAsync(RequestTask<IActionResult> task) =>
        _implementation.WithErrorHandlingAsync(task);

    /// <inheritdoc cref="ErrorHandlingManager.WithErrorHandlingAsync{TResult}(RequestTask{TResult})"/>/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected virtual Task WithErrorHandlingAsync(RequestTask task) =>
        _implementation.WithErrorHandlingAsync(task);

    /// <inheritdoc cref="ErrorHandlingManager.WithErrorHandlingAsync{TResult}(RequestTask{TResult})"/>/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected virtual Task<TResult> WithErrorHandlingAsync<TResult>(RequestTask<TResult> task) =>
        _implementation.WithErrorHandlingAsync(task);

    /// <inheritdoc cref="ErrorHandlingManager.AssertModelState"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected virtual void AssertModelState() => _implementation.AssertModelState();

    /// <inheritdoc cref="ErrorHandlingManager.OnError"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected virtual void OnError(Exception e) => _implementation.OnError(e);

    /// <inheritdoc cref="ErrorHandlingManager.AfterHandled"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected virtual ApiProxyException AfterHandled(Exception e) => _implementation.AfterHandled(e);
}
