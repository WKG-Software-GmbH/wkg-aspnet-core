namespace Wkg.AspNetCore.Authentication.Jwt;

/// <summary>
/// Represents the options for claim validation.
/// </summary>
public interface IClaimValidationOptions
{
    /// <summary>
    /// The duration for which claims are considered valid.
    /// </summary>
    TimeSpan TimeToLive { get; }
}
