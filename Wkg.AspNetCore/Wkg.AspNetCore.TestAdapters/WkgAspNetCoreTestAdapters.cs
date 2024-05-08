namespace Wkg.AspNetCore.TestAdapters;

/// <summary>
/// Provides version information for the Wkg.AspNetCore.TestAdapters framework.
/// </summary>
public static class WkgAspNetCoreTestAdapters
{
    private const string __CI_DEPLOYMENT_VERSION_PREFIX = "0.0.0";
    private const string __CI_DEPLOYMENT_VERSION_SUFFIX = "CI-INJECTED";

    private static Version? _version;
    private static string? _versionString;

    /// <summary>
    /// Retrives a <see cref="System.Version"/> object representing the version of the Wkg.AspNetCore.TestAdapters framework.
    /// </summary>
    public static Version Version => _version ??= new Version(__CI_DEPLOYMENT_VERSION_PREFIX);

    /// <summary>
    /// Indicates whether the current version of the framework is a pre-release version.
    /// </summary>
    public static bool IsPreRelease => !string.IsNullOrWhiteSpace(__CI_DEPLOYMENT_VERSION_SUFFIX);

    /// <summary>
    /// Retrieves the pre-release tag for the current version of the framework, if any.
    /// </summary>
    public static string? PreReleaseTag => string.IsNullOrWhiteSpace(__CI_DEPLOYMENT_VERSION_SUFFIX) ? null : __CI_DEPLOYMENT_VERSION_SUFFIX;

    /// <summary>
    /// Retrieves the raw version string for the current version of the framework using semantic versioning in the format <c>major.minor.patch-prerelease</c>.
    /// </summary>
    public static string VersionString => _versionString ??= string.IsNullOrWhiteSpace(__CI_DEPLOYMENT_VERSION_SUFFIX)
        ? __CI_DEPLOYMENT_VERSION_PREFIX
        : $"{__CI_DEPLOYMENT_VERSION_PREFIX}-{__CI_DEPLOYMENT_VERSION_SUFFIX}";
}
