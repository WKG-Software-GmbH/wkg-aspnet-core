using System.Diagnostics;
using Wkg.Logging;

namespace Wkg.AspNetCore.Abstractions.Managers.Results;

/// <summary>
/// Represents the result of a manager operation with no return value.
/// </summary>
public readonly struct ManagerResult
{
    /// <summary>
    /// Gets the status code of the result.
    /// </summary>
    public readonly ManagerResultCode StatusCode;

    /// <summary>
    /// Gets the error message of the result, if any.
    /// </summary>
    public readonly string? ErrorMessage;

    internal ManagerResult(ManagerResultCode statusCode, string? errorMessage)
    {
        StatusCode = statusCode;
        ErrorMessage = errorMessage;
    }

    /// <summary>
    /// Indicates whether the result is empty, i.e., was never initialized.
    /// </summary>
    public bool IsEmpty => StatusCode == ManagerResultCode.Unknown;

    /// <summary>
    /// Indicates whether the result is successful.
    /// </summary>
    public bool IsSuccess => StatusCode == ManagerResultCode.Success;

    /// <summary>
    /// Converts this failed result into a corresponding failed result of type <typeparamref name="TOther"/>.
    /// </summary>
    /// <typeparam name="TOther">The type of the result to convert to.</typeparam>
    public readonly ManagerResult<TOther> FailureAs<TOther>()
    {
        if (IsEmpty)
        {
            string message = "You are attempting to cast an empty result to an unsuccessful one. I'm not sure if that's what you want.";
            Log.WriteWarning(message);
            Debug.Fail(message);
        }
        else if (IsSuccess)
        {
            string message = "You are attempting to cast a successful result to an unsuccessful one. I'm not sure if that's what you want.";
            Log.WriteWarning(message);
            Debug.Fail(message);
        }
        return new ManagerResult<TOther>(this, default!);
    }

    /// <summary>
    /// Creates a new successful result.
    /// </summary>
    public static ManagerResult Success() => new(ManagerResultCode.Success, null);

    /// <summary>
    /// Creates a new <see cref="ManagerResultCode.NotFound"/> result with the specified error message.
    /// </summary>
    /// <param name="errorMessage">The error message describing the reason for the failure.</param>
    public static ManagerResult NotFound(string? errorMessage = null) => new(ManagerResultCode.NotFound, errorMessage);

    /// <summary>
    /// Creates a new <see cref="ManagerResultCode.BadRequest"/> result with the specified error message.
    /// </summary>
    /// <param name="errorMessage">The error message describing the reason for the failure.</param>
    public static ManagerResult BadRequest(string? errorMessage = null) => new(ManagerResultCode.BadRequest, errorMessage);

    /// <summary>
    /// Creates a new <see cref="ManagerResultCode.Unauthorized"/> result with the specified error message.
    /// </summary>
    /// <param name="errorMessage">The error message describing the reason for the failure.</param>
    public static ManagerResult Unauthorized(string? errorMessage = null) => new(ManagerResultCode.Unauthorized, errorMessage);

    /// <summary>
    /// Creates a new <see cref="ManagerResultCode.Forbidden"/> result with the specified error message.
    /// </summary>
    /// <param name="errorMessage">The error message describing the reason for the failure.</param>
    public static ManagerResult Forbidden(string? errorMessage = null) => new(ManagerResultCode.Forbidden, errorMessage);

    /// <summary>
    /// Creates a new <see cref="ManagerResultCode.InternalServerError"/> result with the specified error message.
    /// </summary>
    /// <param name="errorMessage">The error message describing the reason for the failure.</param>
    public static ManagerResult InternalServerError(string errorMessage) => new(ManagerResultCode.InternalServerError, errorMessage);

    /// <summary>
    /// Creates a new successful result with the specified value.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="value">The value of the result.</param>
    public static ManagerResult<TResult> Success<TResult>(TResult value) => new(Success(), value);

    /// <summary>
    /// Creates a new <see cref="ManagerResultCode.NotFound"/> result with the specified error message.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="errorMessage">The error message describing the reason for the failure.</param>
    public static ManagerResult<TResult> NotFound<TResult>(string? errorMessage = null) => NotFound(errorMessage).FailureAs<TResult>();

    /// <summary>
    /// Creates a new <see cref="ManagerResultCode.BadRequest"/> result with the specified error message.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="errorMessage">The error message describing the reason for the failure.</param>
    public static ManagerResult<TResult> BadRequest<TResult>(string? errorMessage = null) => BadRequest(errorMessage).FailureAs<TResult>();

    /// <summary>
    /// Creates a new <see cref="ManagerResultCode.Unauthorized"/> result with the specified error message.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="errorMessage">The error message describing the reason for the failure.</param>
    public static ManagerResult<TResult> Unauthorized<TResult>(string? errorMessage = null) => Unauthorized(errorMessage).FailureAs<TResult>();

    /// <summary>
    /// Creates a new <see cref="ManagerResultCode.Forbidden"/> result with the specified error message.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="errorMessage">The error message describing the reason for the failure.</param>
    public static ManagerResult<TResult> Forbidden<TResult>(string? errorMessage = null) => Forbidden(errorMessage).FailureAs<TResult>();

    /// <summary>
    /// Creates a new <see cref="ManagerResultCode.InternalServerError"/> result with the specified error message.
    /// </summary>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <param name="errorMessage">The error message describing the reason for the failure.</param>
    public static ManagerResult<TResult> InternalServerError<TResult>(string errorMessage) => InternalServerError(errorMessage).FailureAs<TResult>();
}