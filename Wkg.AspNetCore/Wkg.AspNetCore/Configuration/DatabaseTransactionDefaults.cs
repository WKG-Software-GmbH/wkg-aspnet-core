using System.Data;

namespace Wkg.AspNetCore.Configuration;

internal static class DatabaseTransactionDefaults
{
    public static IsolationLevel DefaultIsolationLevel { get; set; } = IsolationLevel.ReadCommitted;
}
