namespace Wkg.AspNetCore.Abstractions.Internals;

internal interface IErrorState
{
    string? Details { get; }

    static abstract IErrorState CreateErrorState(string message);
}
