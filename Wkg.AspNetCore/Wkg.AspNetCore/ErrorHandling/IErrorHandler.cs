using Microsoft.AspNetCore.Mvc;
using Wkg.AspNetCore.Exceptions;
using Wkg.AspNetCore.RequestActions;

namespace Wkg.AspNetCore.ErrorHandling;

public interface IErrorHandler
{
    /// <summary>
    /// Performs the necessary actions to handle the intercepted <see cref="Exception"/> and returns an <see cref="ApiProxyException"/> corresponding to the intercepted <see cref="Exception"/>.
    /// </summary>
    /// <param name="e">The intercepted <see cref="Exception"/>.</param>
    /// <returns>An <see cref="ApiProxyException"/> corresponding to the intercepted <see cref="Exception"/>.</returns>
    ApiProxyException AfterHandled(Exception e);

    /// <summary>
    /// Executes the specified <paramref name="action"/> with error handling and returns the result.
    /// </summary>
    /// <param name="action">The action to be executed with error handling.</param>
    /// <returns>The result of the specified <paramref name="action"/>.</returns>
    /// <exception cref="ApiProxyException"> if an exception occurs during the execution of the specified <paramref name="action"/>.</exception>
    IActionResult Try(RequestAction<IActionResult> action);

    /// <summary>
    /// Executes the specified <paramref name="action"/> with error handling.
    /// </summary>
    /// <param name="action">The action to be executed with error handling.</param>
    /// <exception cref="ApiProxyException"> if an exception occurs during the execution of the specified <paramref name="action"/>.</exception>
    void Try(RequestAction action);

    /// <summary>
    /// Executes the specified <paramref name="action"/> with error handling and returns the result.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="action">The action to be executed with error handling.</param>
    /// <returns>The result of the specified <paramref name="action"/>.</returns>
    /// <exception cref="ApiProxyException"> if an exception occurs during the execution of the specified <paramref name="action"/>.</exception>
    TResult Try<TResult>(RequestAction<TResult> action);

    /// <summary>
    /// Executes the specified <paramref name="task"/> with error handling and returns the result.
    /// </summary>
    /// <param name="task">The action to be executed with error handling.</param>
    /// <returns>The asynchronous result of the specified <paramref name="task"/>.</returns>
    /// <exception cref="ApiProxyException"> if an exception occurs during the execution of the specified <paramref name="task"/>.</exception>
    Task<IActionResult> TryAsync(RequestTask<IActionResult> task);

    /// <summary>
    /// Executes the specified <paramref name="task"/> with error handling.
    /// </summary>
    /// <param name="task">The action to be executed with error handling.</param>
    /// <returns>The <see cref="Task"/> representing the asynchronous action being performed.</returns>
    /// <exception cref="ApiProxyException"> if an exception occurs during the execution of the specified <paramref name="task"/>.</exception>
    Task TryAsync(RequestTask task);

    /// <summary>
    /// Executes the specified <paramref name="task"/> with error handling and returns the result.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="task">The action to be executed with error handling.</param>
    /// <returns>The asynchronous result of the specified <paramref name="task"/>.</returns>
    /// <exception cref="ApiProxyException"> if an exception occurs during the execution of the specified <paramref name="task"/>.</exception>
    Task<TResult> TryAsync<TResult>(RequestTask<TResult> task);
}
