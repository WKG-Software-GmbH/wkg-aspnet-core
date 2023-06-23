using System.Data;

namespace Wkg.AspNetCore.Configuration;

internal static class DatabaseControllerDefaults
{
    public static IsolationLevel DefaultIsolationLevel { get; set; } = IsolationLevel.ReadCommitted;
}
