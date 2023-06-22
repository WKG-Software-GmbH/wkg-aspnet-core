using Wkg.AspNetCore.Controllers;

namespace Wkg.AspNetCore.Exceptions;

/// <summary>
/// Represents an <see cref="Exception"/> that is (re-)thrown when an unhandled error was intercepted by an <see cref="ErrorHandlingController"/> instance.
/// </summary>
public class ApiProxyException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ApiProxyException"/> class.
    /// </summary>
    /// <param name="innerException">The inner exception that caused this exception to be thrown.</param>
    public ApiProxyException(Exception innerException) : base(nameof(ApiProxyException), innerException)
    {
    }

    /// <summary>
    /// Gets the message that describes the current exception.
    /// </summary>
    public override string? StackTrace => InnerException!.StackTrace;
}