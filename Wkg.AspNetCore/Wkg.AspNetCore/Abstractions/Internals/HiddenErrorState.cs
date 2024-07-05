namespace Wkg.AspNetCore.Abstractions.Internals;

internal class HiddenErrorState : IErrorState
{
    private static readonly HiddenErrorState _instance = new();

    public string? Details => null;

    public static IErrorState CreateErrorState(string message) => _instance;
}