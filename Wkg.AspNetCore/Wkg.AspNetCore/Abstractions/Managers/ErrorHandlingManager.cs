using Microsoft.AspNetCore.Mvc;
using Wkg.AspNetCore.Exceptions;
using Wkg.AspNetCore.RequestActions;
using Wkg.Logging;

namespace Wkg.AspNetCore.Abstractions.Managers;

/// <summary>
/// Provides a base class for ASP managers to handle exceptions.
/// </summary>
public abstract class ErrorHandlingManager : ManagerBase
{
    /// <summary>
    /// Indicates whether the application is running in development mode and if stack traces should be appended to the Model State.
    /// </summary>
    protected bool IsDevelopmentMode { get; set; } = false;

    /// <summary>
    /// Executes the specified <paramref name="action"/> with error handling and returns the result.
    /// </summary>
    /// <param name="action">The action to be executed with error handling.</param>
    /// <returns>The result of the specified <paramref name="action"/>.</returns>
    /// <exception cref="ApiProxyException"> if an exception occurs during the execution of the specified <paramref name="action"/>.</exception>
    internal protected virtual IActionResult WithErrorHandling(RequestAction<IActionResult> action) =>
        WithErrorHandling<IActionResult>(action);

    /// <summary>
    /// Executes the specified <paramref name="action"/> with error handling.
    /// </summary>
    /// <param name="action">The action to be executed with error handling.</param>
    /// <exception cref="ApiProxyException"> if an exception occurs during the execution of the specified <paramref name="action"/>.</exception>
    internal protected virtual void WithErrorHandling(RequestAction action) => WithErrorHandling<VoidResult>(() =>
    {
        action.Invoke();
        return default;
    });

    /// <summary>
    /// Executes the specified <paramref name="action"/> with error handling and returns the result.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="action">The action to be executed with error handling.</param>
    /// <returns>The result of the specified <paramref name="action"/>.</returns>
    /// <exception cref="ApiProxyException"> if an exception occurs during the execution of the specified <paramref name="action"/>.</exception>
    internal protected virtual TResult WithErrorHandling<TResult>(RequestAction<TResult> action)
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
    internal protected virtual Task<IActionResult> WithErrorHandlingAsync(RequestTask<IActionResult> task) =>
        WithErrorHandlingAsync<IActionResult>(task);

    /// <summary>
    /// Executes the specified <paramref name="task"/> with error handling.
    /// </summary>
    /// <param name="task">The action to be executed with error handling.</param>
    /// <returns>The <see cref="Task"/> representing the asynchronous action being performed.</returns>
    /// <exception cref="ApiProxyException"> if an exception occurs during the execution of the specified <paramref name="task"/>.</exception>
    internal protected virtual Task WithErrorHandlingAsync(RequestTask task) => WithErrorHandlingAsync<VoidResult>(async () =>
    {
        await task.Invoke();
        return default;
    });

    /// <summary>
    /// Executes the specified <paramref name="task"/> with error handling and returns the result.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="task">The action to be executed with error handling.</param>
    /// <returns>The asynchronous result of the specified <paramref name="task"/>.</returns>
    /// <exception cref="ApiProxyException"> if an exception occurs during the execution of the specified <paramref name="task"/>.</exception>
    internal protected virtual async Task<TResult> WithErrorHandlingAsync<TResult>(RequestTask<TResult> task)
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
    internal protected virtual void AssertModelState()
    {
        if (Context?.ModelState.IsValid is false)
        {
            throw new InvalidOperationException($"{nameof(Context.ModelState)} was invalid!");
        }
    }

    /// <summary>
    /// Performs the necessary actions to handle the intercepted <see cref="Exception"/>.
    /// </summary>
    /// <param name="e">The intercepted <see cref="Exception"/>.</param>
    /// <remarks>
    /// By default, the <see cref="Exception"/> is logged and returned to the client via a model state error.
    /// </remarks>
    internal protected virtual void OnError(Exception e)
    {
        const string stackTraceNull = "<UnknownStackTrace>";

        if (IsDevelopmentMode && Context?.ModelState is not null)
        {
            Context.ModelState.AddModelError("ExceptionMessage", e.Message);
            Context.ModelState.AddModelError("StackTrace", e.StackTrace ?? stackTraceNull);
            if (e.InnerException is not null)
            {
                Context.ModelState.AddModelError("InnerException", e.InnerException.Message);
                Context.ModelState.AddModelError("InnerExceptionStackTrace", e.InnerException.StackTrace ?? stackTraceNull);
            }
        }

        // write to all configured loggers
        Log.WriteException(e, LogLevel.Fatal);
    }

    /// <summary>
    /// Performs the necessary actions to handle the intercepted <see cref="Exception"/> and returns an <see cref="ApiProxyException"/> corresponding to the intercepted <see cref="Exception"/>.
    /// </summary>
    /// <param name="e">The intercepted <see cref="Exception"/>.</param>
    /// <returns>An <see cref="ApiProxyException"/> corresponding to the intercepted <see cref="Exception"/>.</returns>
    internal protected virtual ApiProxyException AfterHandled(Exception e)
    {
        OnError(e);
        // preserve original stacktrace
        return new ApiProxyException(e);
    }
}
