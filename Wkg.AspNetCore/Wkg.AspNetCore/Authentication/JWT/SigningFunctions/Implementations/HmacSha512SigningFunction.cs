using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using Wkg.Data.Pooling;

namespace Wkg.AspNetCore.Authentication.Jwt.SigningFunctions.Implementations;

internal class HmacSha512SigningFunction(HmacOptions options) : IJwtSigningFunction
{
    public string Name => "HS512";

    public int SignatureLengthInBytes => HMACSHA512.HashSizeInBytes;

    public int Sign(ISessionKey sessionKey, ReadOnlySpan<byte> buffer, Span<byte> signature)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(signature.Length, HMACSHA512.HashSizeInBytes, nameof(signature));

        // allocate buffer for HMAC key data (server secret + session key)
        PooledArray<byte> keyBuffer = ArrayPool.Rent<byte>(options.SecretBytes.Length + sessionKey.Size);
        Span<byte> keyBufferSpan = keyBuffer.AsSpan();
        // copy server secret and session key into buffer
        Unsafe.CopyBlock(ref keyBufferSpan[0], in options.SecretBytes[0], (uint)options.SecretBytes.Length);
        // we can directly write the session key into the correct position in the buffer
        sessionKey.WriteKey(keyBufferSpan[^sessionKey.Size..]);
        // compute Hash-based Message Authentication Code (HMAC) using server secret and session key
        int bytesWritten = HMACSHA512.HashData(keyBufferSpan, buffer, signature);
        ArrayPool.Return(keyBuffer, clearArray: true);
        return bytesWritten;
    }

    public int Sign(ISessionKey sessionKey, Stream stream, Span<byte> signature)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(signature.Length, HMACSHA512.HashSizeInBytes, nameof(signature));

        // don't forget to reset the Stream position to avoid computing the HMAC over 0 bytes
        if (stream.Position != 0 && stream.CanSeek)
        {
            stream.Position = 0;
        }
        // allocate buffer for HMAC key data (server secret + session key)
        PooledArray<byte> keyBuffer = ArrayPool.Rent<byte>(options.SecretBytes.Length + sessionKey.Size);
        Span<byte> keyBufferSpan = keyBuffer.AsSpan();
        // copy server secret and session key into buffer
        Unsafe.CopyBlock(ref keyBufferSpan[0], in options.SecretBytes[0], (uint)options.SecretBytes.Length);
        // we can directly write the session key into the correct position in the buffer
        sessionKey.WriteKey(keyBufferSpan[^sessionKey.Size..]);
        // compute Hash-based Message Authentication Code (HMAC) using server secret and session key
        int bytesWritten = HMACSHA512.HashData(keyBufferSpan, stream, signature);
        ArrayPool.Return(keyBuffer, clearArray: true);
        return bytesWritten;
    }
}
