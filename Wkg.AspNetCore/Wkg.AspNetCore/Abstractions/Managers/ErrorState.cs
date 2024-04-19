namespace Wkg.AspNetCore.Abstractions.Managers;

internal interface IErrorState
{
    string? Details { get; }

    static abstract IErrorState CreateErrorState(string message);
}

internal record ErrorState : IErrorState
{
    public required string? Details { get; init; }

    public static IErrorState CreateErrorState(string? message) => new ErrorState { Details = message };
}

internal class HiddenErrorState : IErrorState
{
    private static readonly HiddenErrorState _instance = new();

    public string? Details => null;

    public static IErrorState CreateErrorState(string message) => _instance;
}