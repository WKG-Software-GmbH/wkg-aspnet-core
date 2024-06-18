namespace Wkg.AspNetCore.Authentication;

public interface IClaimValidationOptions
{
    TimeSpan TimeToLive { get; }
}
