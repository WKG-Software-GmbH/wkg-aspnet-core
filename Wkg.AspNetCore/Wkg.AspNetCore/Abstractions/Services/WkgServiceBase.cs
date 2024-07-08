using Wkg.AspNetCore.ErrorHandling;

namespace Wkg.AspNetCore.Abstractions.Services;

/// <summary>
/// Provides a base class for dependency injection services.
/// </summary>
/// <remarks>
/// Creates a new instance of the <see cref="WkgServiceBase"/> class.
/// </remarks>
/// <param name="errorSentry">The DI descriptor of the error sentry.</param>
public abstract class WkgServiceBase(IErrorSentry errorSentry)
{
    /// <summary>
    /// Gets the <see cref="IErrorSentry"/> associated with this context.
    /// </summary>
    protected IErrorSentry ErrorSentry { get; } = errorSentry;
}