using System.Text;

namespace Wkg.AspNetCore.Authentication.Jwt.SigningFunctions;

internal class HmacOptions(string secret)
{
    public byte[] SecretBytes { get; } = Encoding.UTF8.GetBytes(secret);
}