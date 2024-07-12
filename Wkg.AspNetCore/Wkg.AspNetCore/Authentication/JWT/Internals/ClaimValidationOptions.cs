namespace Wkg.AspNetCore.Authentication.Jwt.Internals;

internal record ClaimValidationOptions(TimeSpan TimeToLive) : IClaimValidationOptions;