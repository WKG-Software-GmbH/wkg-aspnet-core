using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.CodeAnalysis;
using Wkg.AspNetCore.Abstractions.Internals;
using Wkg.AspNetCore.Abstractions.Managers.Results;
using Wkg.AspNetCore.ErrorHandling;

namespace Wkg.AspNetCore.Abstractions.Controllers;

/// <summary>
/// Provides a base class for API controllers to handle exceptions.
/// </summary>
public abstract class WkgControllerBase(IErrorSentry errorSentry) : ControllerBase, IMvcContext
{
    /// <summary>
    /// Gets the <see cref="IErrorSentry"/> associated with this context.
    /// </summary>
    protected IErrorSentry ErrorSentry { get; } = errorSentry;

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
            ManagerResultCode.Unauthorized => Unauthorized(details),
            ManagerResultCode.Forbidden => Forbid(),
            ManagerResultCode.InvalidModelState => BadRequest(ModelState),
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
            ManagerResultCode.Success => Ok(),
            ManagerResultCode.BadRequest => BadRequest(details),
            ManagerResultCode.Unauthorized => Unauthorized(details),
            ManagerResultCode.Forbidden => Forbid(),
            ManagerResultCode.InvalidModelState => BadRequest(ModelState),
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