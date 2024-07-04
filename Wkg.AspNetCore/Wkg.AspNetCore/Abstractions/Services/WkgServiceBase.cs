using Wkg.AspNetCore.ErrorHandling;

namespace Wkg.AspNetCore.Abstractions.Services;

public abstract class WkgServiceBase(IErrorSentry errorSentry)
{
    protected IErrorSentry ErrorSentry { get; } = errorSentry;
}