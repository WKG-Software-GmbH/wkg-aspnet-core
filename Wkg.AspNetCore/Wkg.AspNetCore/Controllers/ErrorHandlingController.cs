using Microsoft.AspNetCore.Mvc;
using Wkg.AspNetCore.Exceptions;
using Wkg.Logging;
using Wkg.AspNetCore.RequestActions;
using System.Runtime.CompilerServices;

namespace Wkg.AspNetCore.Controllers;

public abstract class ErrorHandlingController : ControllerBase
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected virtual IActionResult WithErrorHandling(RequestAction<IActionResult> action) =>
        WithErrorHandling<IActionResult>(action);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected virtual void WithErrorHandling(RequestAction action) => WithErrorHandling(() =>
    {
        action.Invoke();
        return Ok();
    });

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

    protected virtual Task<IActionResult> WithErrorHandlingAsync(RequestTask<IActionResult> action) =>
        WithErrorHandlingAsync<IActionResult>(action);

    protected virtual Task WithErrorHandlingAsync(RequestTask task) => WithErrorHandlingAsync(async () =>
    {
        await task.Invoke();
        return Ok();
    });

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

    protected virtual void AssertModelState()
    {
        if (!ModelState.IsValid)
        {
            throw new InvalidOperationException($"{nameof(ModelState)} was invalid!");
        }
    }

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
