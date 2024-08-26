namespace Wkg.AspNetCore.Authentication.Jwt.SigningFunctions.Implementations;

/// <summary>
/// https://www.rfc-editor.org/rfc/rfc7518#section-3
/// </summary>
internal interface IJwtSigningFunction
{
    string Name { get; }

    int SignatureLengthInBytes { get; }

    int Sign(ISessionKey sessionKey, ReadOnlySpan<byte> buffer, Span<byte> signature);

    int Sign(ISessionKey sessionKey, Stream stream, Span<byte> signature);
}
