namespace Wkg.AspNetCore.Abstractions.Internals;

internal class HiddenErrorState : IErrorState
{
    private static readonly HiddenErrorState s_instance = new();

    public string? Details => null;

    public static IErrorState CreateErrorState(string message) => s_instance;
}