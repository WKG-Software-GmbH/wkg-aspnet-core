using System.Buffers;
using System.Buffers.Text;
using System.Diagnostics;
using Wkg.Data.Pooling;

namespace Wkg.AspNetCore.Authentication.JWT;

internal static class Base64UrlEncoder
{
    public static bool TryBase64UrlDecodeInPlace(Span<byte> input, out Span<byte> output)
    {
        // url decode the input data in-place
        for (int i = 0; i < input.Length; i++)
        {
            byte b = input[i];
            if (b == '-')
            {
                input[i] = (byte)'+';
            }
            else if (b == '_')
            {
                input[i] = (byte)'/';
            }
        }
        OperationStatus status = Base64.DecodeFromUtf8InPlace(input, out int base64decodedLength);
        if (status is not OperationStatus.Done)
        {
            output = default;
            return false;
        }
        // the decoded length is always smaller than the encoded length, so crop it to the actual length
        output = input[..base64decodedLength];
        return true;
    }

    public static PooledArray<byte> Base64UrlEncode(ReadOnlySpan<byte> input)
    {
        // encoding is an inflating operation, so determine the maximum possible size
        int base64CharCount = Base64.GetMaxEncodedToUtf8Length(input.Length);
        // rent a large enough buffer to hold the encoded data
        PooledArray<byte> buffer = ArrayPool.Rent<byte>(base64CharCount);
        // encode the input data to the buffer
        OperationStatus status = Base64.EncodeToUtf8(input, buffer.AsSpan(), out int bytesConsumed, out int bytesWritten, isFinalBlock: true);
        Debug.Assert(status == OperationStatus.Done);
        Debug.Assert(bytesConsumed == input.Length);
        Debug.Assert(bytesWritten <= base64CharCount);
        // clamp the buffer to the actual length
        _ = buffer.TryResizeUnsafe(bytesWritten, out buffer);
        // url encoding is a simple substitution of characters
        Span<byte> resultSpan = buffer.AsSpan();
        for (int i = 0; i < resultSpan.Length; i++)
        {
            byte b = resultSpan[i];
            if (b == '+')
            {
                resultSpan[i] = (byte)'-';
            }
            else if (b == '/')
            {
                resultSpan[i] = (byte)'_';
            }
        }
        return buffer;
    }
}