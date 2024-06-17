namespace Wkg.AspNetCore.Authentication.Internals;

internal readonly struct NoExtendedKeys : IExtendedKeys<NoExtendedKeys>
{
    public static NoExtendedKeys Generate() => default;
}