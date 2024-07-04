using Wkg.AspNetCore.ErrorHandling;

namespace Wkg.AspNetCore.Abstractions.Services;

public abstract class WkgServiceBase(IErrorHandler errorHandler)
{
    protected IErrorHandler ErrorHandler { get; } = errorHandler;
}
