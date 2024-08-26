using Microsoft.AspNetCore.Mvc;
using Wkg.AspNetCore.Exceptions;
using Wkg.AspNetCore.RequestActions;

namespace Wkg.AspNetCore.ErrorHandling;

/// <summary>
/// Provides means to execute actions in a monitored scope, intercepting exceptions and handling them according to the underlying implementation.
/// </summary>
/// <remarks>
/// Common use cases include intercepting exceptions, logging, wrapping, and rethrowing.
/// </remarks>
public interface IErrorSentry
{
    /// <summary>
    /// Performs the necessary actions to handle the intercepted <see cref="Exception"/> and returns an <see cref="ApiProxyException"/> corresponding to the intercepted <see cref="Exception"/>.
    /// </summary>
    /// <param name="e">The intercepted <see cref="Exception"/>.</param>
    /// <returns>An <see cref="ApiProxyException"/> corresponding to the intercepted <see cref="Exception"/>.</returns>
    ApiProxyException AfterHandled(Exception e);

    /// <summary>
    /// Executes the specified <paramref name="action"/>, observes and intercepts any unhandled exceptions, and returns the result.
    /// </summary>
    /// <param name="action">The action to be executed in a monitored scope.</param>
    /// <returns>The result of the specified <paramref name="action"/>.</returns>
    /// <exception cref="ApiProxyException"> if an exception occurs during the execution of the specified <paramref name="action"/>.</exception>
    IActionResult Watch(RequestAction<IActionResult> action);

    /// <summary>
    /// Executes the specified <paramref name="action"/> and observes and intercepts any unhandled exceptions.
    /// </summary>
    /// <param name="action">The action to be executed in a monitored scope.</param>
    /// <exception cref="ApiProxyException"> if an exception occurs during the execution of the specified <paramref name="action"/>.</exception>
    void Watch(RequestAction action);

    /// <summary>
    /// Executes the specified <paramref name="action"/>, observes and intercepts any unhandled exceptions, and returns the result.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="action">The action to be executed in a monitored scope.</param>
    /// <returns>The result of the specified <paramref name="action"/>.</returns>
    /// <exception cref="ApiProxyException"> if an exception occurs during the execution of the specified <paramref name="action"/>.</exception>
    TResult Watch<TResult>(RequestAction<TResult> action);

    /// <summary>
    /// Executes the specified <paramref name="task"/>, observes and intercepts any unhandled exceptions, and returns the result.
    /// </summary>
    /// <param name="task">The action to be executed in a monitored scope.</param>
    /// <returns>The asynchronous result of the specified <paramref name="task"/>.</returns>
    /// <exception cref="ApiProxyException"> if an exception occurs during the execution of the specified <paramref name="task"/>.</exception>
    Task<IActionResult> WatchAsync(RequestTask<IActionResult> task);

    /// <summary>
    /// Executes the specified <paramref name="task"/> and observes and intercepts any unhandled exceptions.
    /// </summary>
    /// <param name="task">The action to be executed in a monitored scope.</param>
    /// <returns>The <see cref="Task"/> representing the asynchronous action being performed.</returns>
    /// <exception cref="ApiProxyException"> if an exception occurs during the execution of the specified <paramref name="task"/>.</exception>
    Task WatchAsync(RequestTask task);

    /// <summary>
    /// Executes the specified <paramref name="task"/>, observes and intercepts any unhandled exceptions, and returns the result.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="task">The action to be executed in a monitored scope.</param>
    /// <returns>The asynchronous result of the specified <paramref name="task"/>.</returns>
    /// <exception cref="ApiProxyException"> if an exception occurs during the execution of the specified <paramref name="task"/>.</exception>
    Task<TResult> WatchAsync<TResult>(RequestTask<TResult> task);
}
