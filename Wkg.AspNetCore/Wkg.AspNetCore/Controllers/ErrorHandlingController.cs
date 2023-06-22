using Microsoft.AspNetCore.Mvc;
using Wkg.AspNetCore.Exceptions;
using Wkg.Logging;
using Wkg.AspNetCore.RequestActions;
using System.Runtime.CompilerServices;

namespace Wkg.AspNetCore.Controllers;

/// <summary>
/// Provides a base class for API controllers to handle exceptions.
/// </summary>
public abstract class ErrorHandlingController : ControllerBase
{
    /// <summary>
    /// Executes the specified <paramref name="action"/> with error handling and returns the result.
    /// </summary>
    /// <param name="action">The action to be executed with error handling.</param>
    /// <returns>The result of the specified <paramref name="action"/>.</returns>
    /// <exception cref="ApiProxyException"> if an exception occurs during the execution of the specified <paramref name="action"/>.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected virtual IActionResult WithErrorHandling(RequestAction<IActionResult> action) =>
        WithErrorHandling<IActionResult>(action);

    /// <summary>
    /// Executes the specified <paramref name="action"/> with error handling.
    /// </summary>
    /// <param name="action">The action to be executed with error handling.</param>
    /// <exception cref="ApiProxyException"> if an exception occurs during the execution of the specified <paramref name="action"/>.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected virtual void WithErrorHandling(RequestAction action) => WithErrorHandling(() =>
    {
        action.Invoke();
        return Ok();
    });

    /// <summary>
    /// Executes the specified <paramref name="action"/> with error handling and returns the result.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="action">The action to be executed with error handling.</param>
    /// <returns>The result of the specified <paramref name="action"/>.</returns>
    /// <exception cref="ApiProxyException"> if an exception occurs during the execution of the specified <paramref name="action"/>.</exception>
    protected virtual TResult WithErrorHandling<TResult>(RequestAction<TResult> action)
    {
        try
        {
            return action.Invoke();
        }
        catch (Exception e)
        {
            throw AfterHandled(e);
        }
    }

    /// <summary>
    /// Executes the specified <paramref name="task"/> with error handling and returns the result.
    /// </summary>
    /// <param name="task">The action to be executed with error handling.</param>
    /// <returns>The asynchronous result of the specified <paramref name="task"/>.</returns>
    /// <exception cref="ApiProxyException"> if an exception occurs during the execution of the specified <paramref name="task"/>.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected virtual Task<IActionResult> WithErrorHandlingAsync(RequestTask<IActionResult> task) =>
        WithErrorHandlingAsync<IActionResult>(task);

    /// <summary>
    /// Executes the specified <paramref name="task"/> with error handling.
    /// </summary>
    /// <param name="task">The action to be executed with error handling.</param>
    /// <returns>The <see cref="Task"/> representing the asynchronous action being performed.</returns>
    /// <exception cref="ApiProxyException"> if an exception occurs during the execution of the specified <paramref name="task"/>.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected virtual Task WithErrorHandlingAsync(RequestTask task) => WithErrorHandlingAsync(async () =>
    {
        await task.Invoke();
        return Ok();
    });

    /// <summary>
    /// Executes the specified <paramref name="task"/> with error handling and returns the result.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="task">The action to be executed with error handling.</param>
    /// <returns>The asynchronous result of the specified <paramref name="task"/>.</returns>
    /// <exception cref="ApiProxyException"> if an exception occurs during the execution of the specified <paramref name="task"/>.</exception>
    protected virtual async Task<TResult> WithErrorHandlingAsync<TResult>(RequestTask<TResult> task)
    {
        try
        {
            return await task.Invoke();
        }
        catch (Exception e)
        {
            throw AfterHandled(e);
        }
    }

    /// <summary>
    /// Asserts the <see cref="ControllerBase.ModelState"/> is valid.
    /// </summary>
    /// <exception cref="InvalidOperationException"> if the <see cref="ControllerBase.ModelState"/> is invalid.</exception>
    protected virtual void AssertModelState()
    {
        if (!ModelState.IsValid)
        {
            throw new InvalidOperationException($"{nameof(ModelState)} was invalid!");
        }
    }

    /// <summary>
    /// Performs the necedary actions to handle the intercepted <see cref="Exception"/>.
    /// </summary>
    /// <param name="e">The intercepted <see cref="Exception"/>.</param>
    /// <remarks>
    /// By default, the <see cref="Exception"/> is logged and returned to the client via a model state error.
    /// </remarks>
    protected virtual void OnError(Exception e)
    {
        const string stackTraceNull = "<UnknownStackTrace>";

        ModelState.AddModelError("ExceptionMessage", e.Message);
        ModelState.AddModelError("StackTrace", e.StackTrace ?? stackTraceNull);
        if (e.InnerException is not null)
        {
            ModelState.AddModelError("InnerException", e.InnerException.Message);
            ModelState.AddModelError("InnerExceptionStackTrace", e.InnerException.StackTrace ?? stackTraceNull);
        }
        // write to all configured loggers
        Log.WriteException(e, LogLevel.Fatal);
    }

    private protected virtual ApiProxyException AfterHandled(Exception e)
    {
        OnError(e);
        // preserve original stacktrace
        return new ApiProxyException(e);
    }
}
