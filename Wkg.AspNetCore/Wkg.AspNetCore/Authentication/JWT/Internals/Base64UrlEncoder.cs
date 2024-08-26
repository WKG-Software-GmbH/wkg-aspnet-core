using System.Buffers;
using System.Buffers.Text;
using System.Diagnostics;
using Wkg.Data.Pooling;

namespace Wkg.AspNetCore.Authentication.Jwt.Internals;

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
        // remove partial data at the end of the buffer and decode that separately
        int padding = input.Length % 4;
        Span<byte> inputWithoutPadding = input[..^padding];
        OperationStatus status = Base64.DecodeFromUtf8InPlace(inputWithoutPadding, out int base64decodedLength);
        if (status is not OperationStatus.Done)
        {
            goto FAILURE;
        }
        int paddingDecodedLength = 0;
        if (padding > 0)
        {
            Span<byte> paddingSpan = stackalloc byte[4];
            for (int i = 0; i < paddingSpan.Length; i++)
            {
                if (i < padding)
                {
                    paddingSpan[i] = input[^(padding - i)];
                }
                else
                {
                    paddingSpan[i] = (byte)'=';
                }
            }
            status = Base64.DecodeFromUtf8InPlace(paddingSpan, out paddingDecodedLength);
            if (status is not OperationStatus.Done)
            {
                goto FAILURE;
            }
            paddingSpan[..paddingDecodedLength].CopyTo(input[base64decodedLength..]);
        }
        // the decoded length is always smaller than the encoded length, so crop it to the actual length
        output = input[..(base64decodedLength + paddingDecodedLength)];
        return true;
    FAILURE:
        output = default;
        return false;
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
        // strip padding characters
        int padding = 0;
        for (; padding < resultSpan.Length && resultSpan[^(padding + 1)] == (byte)'='; padding++)
        { }
        _ = buffer.TryResizeUnsafe(resultSpan.Length - padding, out buffer);
        return buffer;
    }
}