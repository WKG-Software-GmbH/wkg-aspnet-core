using Wkg.Versioning;

namespace Wkg.AspNetCore.TestAdapters;

/// <summary>
/// Provides version information for the Wkg.AspNetCore.TestAdapters framework.
/// </summary>
public class WkgAspNetCoreTestAdapters : DeploymentVersionInfo
{
    private const string CI_DEPLOYMENT__VERSION_PREFIX = "0.0.0";
    private const string CI_DEPLOYMENT__VERSION_SUFFIX = "CI-INJECTED";
    private const string CI_DEPLOYMENT__DATETIME_UTC = "1970-01-01 00:00:00";

    private WkgAspNetCoreTestAdapters() : base
    (
        CI_DEPLOYMENT__VERSION_PREFIX,
        CI_DEPLOYMENT__VERSION_SUFFIX,
        CI_DEPLOYMENT__DATETIME_UTC
    ) => Pass();

    /// <summary>
    /// Provides version information for the Wkg.AspNetCore.TestAdapters framework.
    /// </summary>
    public static WkgAspNetCoreTestAdapters VersionInfo { get; } = new WkgAspNetCoreTestAdapters();
}