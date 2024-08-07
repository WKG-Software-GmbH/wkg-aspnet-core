using Wkg.Versioning;

namespace Wkg.AspNetCore;

/// <summary>
/// Provides version information for the Wkg.AspNetCore framework.
/// </summary>
public class WkgAspNetCore : DeploymentVersionInfo
{
    private const string CI_DEPLOYMENT__VERSION_PREFIX = "0.0.0";
    private const string CI_DEPLOYMENT__VERSION_SUFFIX = "CI-INJECTED";
    private const string CI_DEPLOYMENT__DATETIME_UTC = "1970-01-01 00:00:00";

    private WkgAspNetCore() : base
    (
        CI_DEPLOYMENT__VERSION_PREFIX,
        CI_DEPLOYMENT__VERSION_SUFFIX,
        CI_DEPLOYMENT__DATETIME_UTC
    ) => Pass();

    /// <summary>
    /// Provides version information for the Wkg.AspNetCore framework.
    /// </summary>
    public static WkgAspNetCore VersionInfo { get; } = new WkgAspNetCore();
}
