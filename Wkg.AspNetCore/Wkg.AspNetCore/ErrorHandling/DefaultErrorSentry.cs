using Microsoft.AspNetCore.Mvc;
using Wkg.AspNetCore.Exceptions;
using Wkg.AspNetCore.RequestActions;
using Wkg.Logging;

namespace Wkg.AspNetCore.ErrorHandling;

/// <summary>
/// A default implementation of the <see cref="IErrorSentry"/> interface, logging and rethrowing exceptions using the WKG logging framework.
/// </summary>
/// <remarks>
/// This class is intended to be used as a base class for custom error sentry implementations.
/// </remarks>
public class DefaultErrorSentry : IErrorSentry
{
    /// <inheritdoc/>
    public virtual IActionResult Watch(RequestAction<IActionResult> action) =>
        Watch<IActionResult>(action);

    /// <inheritdoc/>
    public virtual void Watch(RequestAction action) => Watch<VoidResult>(() =>
    {
        action.Invoke();
        return default;
    });

    /// <inheritdoc/>
    public virtual TResult Watch<TResult>(RequestAction<TResult> action)
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

    /// <inheritdoc/>
    public virtual Task<IActionResult> WatchAsync(RequestTask<IActionResult> task) =>
        WatchAsync<IActionResult>(task);

    /// <inheritdoc/>
    public virtual Task WatchAsync(RequestTask task) => WatchAsync<VoidResult>(async () =>
    {
        await task.Invoke();
        return default;
    });

    /// <inheritdoc/>
    public async Task<TResult> WatchAsync<TResult>(RequestTask<TResult> task)
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
    /// Performs the necessary actions to handle the intercepted <see cref="Exception"/>.
    /// </summary>
    /// <param name="exception">The intercepted <see cref="Exception"/>.</param>
    /// <remarks>
    /// By default, the <see cref="Exception"/> is logged and returned to the client via a model state error.
    /// </remarks>
    protected virtual void OnError(Exception exception) =>
        // write to all configured loggers
        Log.WriteException(exception, LogLevel.Fatal);

    /// <inheritdoc/>
    public virtual ApiProxyException AfterHandled(Exception e)
    {
        if (e is ApiProxyException apiProxyException)
        {
            return apiProxyException;
        }
        // only log the exception once as it bubbles up the call stack
        OnError(e);
        // preserve original stacktrace
        return new ApiProxyException(e);
    }
}