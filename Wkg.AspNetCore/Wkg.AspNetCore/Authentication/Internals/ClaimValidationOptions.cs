using System.Text;

namespace Wkg.AspNetCore.Authentication.Internals;

public interface IClaimValidationOptions
{
    TimeSpan? Lifetime { get; }
}

internal record ClaimValidationOptions(string Secret, TimeSpan? Lifetime = null) : IClaimValidationOptions
{
    public byte[] SecretBytes { get; } = Encoding.UTF8.GetBytes(Secret);
}
