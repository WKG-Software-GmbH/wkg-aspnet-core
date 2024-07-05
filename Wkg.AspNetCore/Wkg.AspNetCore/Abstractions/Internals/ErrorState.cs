namespace Wkg.AspNetCore.Abstractions.Internals;

internal record ErrorState : IErrorState
{
    public required string? Details { get; init; }

    public static IErrorState CreateErrorState(string? message) => new ErrorState { Details = message };
}
