using System.Text;

namespace Wkg.AspNetCore.Authentication.Internals;

public interface IClaimValidationOptions
{
    TimeSpan? TimeToLive { get; }
}

internal record ClaimValidationOptions(string Secret, TimeSpan? TimeToLive = null) : IClaimValidationOptions
{
    public byte[] SecretBytes { get; } = Encoding.UTF8.GetBytes(Secret);
}
