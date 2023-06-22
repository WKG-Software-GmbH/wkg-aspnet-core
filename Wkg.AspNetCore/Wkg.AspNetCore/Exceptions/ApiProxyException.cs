namespace Wkg.AspNetCore.Exceptions;

public class ApiProxyException : Exception
{
    public ApiProxyException(Exception innerException) : base(nameof(ApiProxyException), innerException)
    {
    }

    public override string? StackTrace => InnerException!.StackTrace;
}