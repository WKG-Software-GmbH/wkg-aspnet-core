namespace Wkg.AspNetCore.Authentication.Internals;

internal readonly struct NoDecryptionKeys : IDecryptionKeys<NoDecryptionKeys>
{
    public static NoDecryptionKeys Generate() => default;
}