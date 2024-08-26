using System.Collections.Immutable;
using System.Text;
using Wkg.Logging;

namespace Wkg.AspNetCore.Authentication.Jwt.Internals;

internal static class HexViewer
{
    private static readonly ImmutableArray<string> s_hexMap;

    static HexViewer()
    {
        string[] hexMap = new string[256];
        for (int i = 0; i < 256; i++)
        {
            hexMap[i] = i.ToString("X2");
        }
        s_hexMap = [.. hexMap];
    }

    public static void PrintBuffer(Span<byte> buffer, string? bufferName = null)
    {
        if (Log.CurrentLogger.MinimumLogLevel > LogLevel.Diagnostic)
        {
            return;
        }
        StringBuilder sb = new();
        sb.AppendLine($"Buffer {bufferName}:");
        const int BYTES_PER_LINE = 16;
        for (int i = 0; i < buffer.Length; i += BYTES_PER_LINE)
        {
            if (i > 0)
            {
                sb.AppendLine();
            }
            for (int j = 0; j < BYTES_PER_LINE && i + j < buffer.Length; j++)
            {
                byte b = buffer[i + j];
                sb.Append(s_hexMap[b]).Append(' ');
            }
            if (i + BYTES_PER_LINE > buffer.Length)
            {
                sb.Append(new string(' ', ((BYTES_PER_LINE - (buffer.Length - i)) * 3) + 4));
            }
            else
            {
                sb.Append("    ");
            }
            for (int j = 0; j < BYTES_PER_LINE && i + j < buffer.Length; j++)
            {
                byte b = buffer[i + j];
                sb.Append(b is >= 32 and <= 126 ? (char)b : '.').Append(' ');
            }
        }
        Log.WriteDiagnostic(sb.ToString());
    }
}