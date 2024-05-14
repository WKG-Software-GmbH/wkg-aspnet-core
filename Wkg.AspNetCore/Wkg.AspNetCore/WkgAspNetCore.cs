﻿using Wkg.Versioning;

namespace Wkg.AspNetCore;

/// <summary>
/// Provides version information for the Wkg.AspNetCore framework.
/// </summary>
public class WkgAspNetCore : DeploymentVersionInfo
{
    private const string __CI_DEPLOYMENT_VERSION_PREFIX = "0.0.0";
    private const string __CI_DEPLOYMENT_VERSION_SUFFIX = "CI-INJECTED";
    private const string __CI_DEPLOYMENT_DATETIME_UTC = "1970-01-01 00:00:00";

    private WkgAspNetCore() : base
    (
        __CI_DEPLOYMENT_VERSION_PREFIX,
        __CI_DEPLOYMENT_VERSION_SUFFIX,
        __CI_DEPLOYMENT_DATETIME_UTC
    ) => Pass();

    /// <summary>
    /// Provides version information for the Wkg.AspNetCore framework.
    /// </summary>
    public static WkgAspNetCore VersionInfo { get; } = new WkgAspNetCore();
}
