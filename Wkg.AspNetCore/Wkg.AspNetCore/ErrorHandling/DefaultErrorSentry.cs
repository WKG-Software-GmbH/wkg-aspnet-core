using Microsoft.AspNetCore.Mvc;
using Wkg.AspNetCore.Exceptions;
using Wkg.AspNetCore.RequestActions;
using Wkg.Logging;

namespace Wkg.AspNetCore.ErrorHandling;

public class DefaultErrorSentry : IErrorSentry
{
    public virtual IActionResult Watch(RequestAction<IActionResult> action) =>
        Watch<IActionResult>(action);

    public virtual void Watch(RequestAction action) => Watch<VoidResult>(() =>
    {
        action.Invoke();
        return default;
    });

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

    public virtual Task<IActionResult> WatchAsync(RequestTask<IActionResult> task) =>
        WatchAsync<IActionResult>(task);

    public virtual Task WatchAsync(RequestTask task) => WatchAsync<VoidResult>(async () =>
    {
        await task.Invoke();
        return default;
    });

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