using System.Text;

namespace Wkg.AspNetCore.Authentication.Internals;

internal record ClaimValidationOptions(string Secret, TimeSpan? Expiration = null)
{
    public byte[] SecretBytes { get; } = Encoding.UTF8.GetBytes(Secret);
}
