﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Wkg.AspNetCore.Abstractions.Controllers;
using Wkg.AspNetCore.Abstractions.Managers;
using Wkg.AspNetCore.Abstractions.Managers.Results;
using Wkg.AspNetCore.Exceptions;
using Wkg.AspNetCore.RequestActions;

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

    /// <summary>
    /// Handles the specified failed <see cref="ManagerResult"/> by returning the appropriate error response.
    /// </summary>
    /// <param name="result">The failed result to handle.</param>
    /// <param name="includeDetails">Indicates whether the error response should include details, such as the error message.
    /// <para><see langword="WARNING"/>: This parameter should be set to <see langword="true"/> only in development environments.</para></param>
    /// <returns>The appropriate error response.</returns>
    /// <exception cref="Exception">thrown when the result is an internal server error, delegating the respose to the error handling middleware.</exception>
    /// <exception cref="InvalidOperationException">thrown when the result is successful, as this method should only be called when the result is not successful.</exception>
    /// <exception cref="ArgumentException">thrown when the result code is not a valid result code.</exception>
    protected virtual IActionResult HandleNonSuccess(ManagerResult result, bool includeDetails = false)
    {
        IErrorState details = includeDetails
            ? ErrorState.CreateErrorState(result.ErrorMessage)
            : HiddenErrorState.CreateErrorState(string.Empty);

        return result.StatusCode switch
        {
            ManagerResultCode.BadRequest => BadRequest(details),
            ManagerResultCode.Unauthorized => Unauthorized(),
            ManagerResultCode.Forbidden => Forbid(),
            ManagerResultCode.InvalidModelState => Page(),
            ManagerResultCode.NotFound => NotFound(details),
            ManagerResultCode.InternalServerError => throw new Exception(result.ErrorMessage), // handled by the error handling middleware
            ManagerResultCode.Success => throw new InvalidOperationException("This method should only be called when the result is not successful."),
            _ => throw new ArgumentException($"{result.StatusCode} is not a valid result code.", nameof(result)),
        };
    }

    /// <summary>
    /// Handles the specified <see cref="ManagerResult"/> by returning the appropriate error response.
    /// </summary>
    /// <param name="result">The result to handle.</param>
    /// <param name="includeDetails">Indicates whether the error response should include details, such as the error message.
    /// <para><see langword="WARNING"/>: This parameter should be set to <see langword="true"/> only in development environments.</para></param>
    /// <returns>The appropriate API response.</returns>
    /// <exception cref="Exception">thrown when the result is an internal server error, delegating the respose to the error handling middleware.</exception>
    /// <exception cref="ArgumentException">thrown when the result code is not a valid result code.</exception>
    protected virtual IActionResult Handle(ManagerResult result, bool includeDetails = false)
    {
        IErrorState details = includeDetails
            ? ErrorState.CreateErrorState(result.ErrorMessage)
            : HiddenErrorState.CreateErrorState(string.Empty);

        return result.StatusCode switch
        {
            ManagerResultCode.Success => Page(),
            ManagerResultCode.BadRequest => BadRequest(details),
            ManagerResultCode.Unauthorized => Unauthorized(),
            ManagerResultCode.Forbidden => Forbid(),
            ManagerResultCode.InvalidModelState => Page(),
            ManagerResultCode.NotFound => NotFound(details),
            ManagerResultCode.InternalServerError => throw new Exception(result.ErrorMessage),
            _ => throw new ArgumentException($"{result.StatusCode} is not a valid result code.", nameof(result)),
        };
    }

    /// <summary>
    /// Attempts to extract the result from the specified <see cref="ManagerResult{TResult}"/>, returning either the result or the appropriate error response.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="result">The result to extract the value from.</param>
    /// <param name="value">When this method returns, contains the result value if the result was successful; otherwise, <see langword="null"/>.</param>
    /// <param name="error">When this method returns, contains the error response if the result was not successful; otherwise, <see langword="null"/>.</param>
    /// <param name="includeDetails">Indicates whether the error response should include details, such as the error message.
    /// <para><see langword="WARNING"/>: This parameter should be set to <see langword="true"/> only in development environments.</para></param>
    /// <returns><see langword="true"/> if the result was successful; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="Exception">thrown when the result is an internal server error, delegating the respose to the error handling middleware.</exception>
    /// <exception cref="ArgumentException">thrown when the result code is not a valid result code.</exception>
    protected virtual bool TryExtractResult<TResult>(ManagerResult<TResult> result, [NotNullWhen(true)] out TResult? value, [NotNullWhen(false)] out IActionResult? error, bool includeDetails = false)
    {
        if (!result.TryGetResult(out value))
        {
            error = HandleNonSuccess(result, includeDetails);
            return false;
        }
        error = null;
        return true;
    }
}
