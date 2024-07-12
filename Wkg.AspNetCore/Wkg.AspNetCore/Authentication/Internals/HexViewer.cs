using System.Collections.Immutable;
using System.Text;
using Wkg.Logging;

namespace Wkg.AspNetCore.Authentication.Internals;

internal static class HexViewer
{
    private static readonly ImmutableArray<string> _hexMap;

    static HexViewer()
    {
        string[] hexMap = new string[256];
        for (int i = 0; i < 256; i++)
        {
            hexMap[i] = i.ToString("X2");
        }
        _hexMap = [.. hexMap];
    }

    public static void PrintBuffer(Span<byte> buffer, string? bufferName = null)
    {
        StringBuilder sb = new();
        sb.AppendLine($"Buffer {bufferName}:");
        const int bytesPerLine = 16;
        for (int i = 0; i < buffer.Length; i += bytesPerLine)
        {
            if (i > 0)
            {
                sb.AppendLine();
            }
            for (int j = 0; j < bytesPerLine && i + j < buffer.Length; j++)
            {
                byte b = buffer[i + j];
                sb.Append(_hexMap[b]).Append(' ');
            }
            if (i + bytesPerLine > buffer.Length)
            {
                sb.Append(new string(' ', (bytesPerLine - (buffer.Length - i)) * 3 + 4));
            }
            else
            {
                sb.Append("    ");
            }
            for (int j = 0; j < bytesPerLine && i + j < buffer.Length; j++)
            {
                byte b = buffer[i + j];
                sb.Append(b is >= 32 and <= 126 ? (char)b : '.').Append(' ');
            }
        }
        Log.WriteDiagnostic(sb.ToString());
    }
}