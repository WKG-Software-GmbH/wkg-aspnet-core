namespace Wkg.AspNetCore.Authentication.Jwt.Internals;

internal readonly struct NoDecryptionKeys : IDecryptionKeys<NoDecryptionKeys>
{
    public static NoDecryptionKeys Generate() => default;
}