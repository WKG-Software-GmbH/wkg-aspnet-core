using System.Text.Json.Serialization;
using Wkg.AspNetCore.Authentication.Jwt.Claims;

namespace Wkg.AspNetCore.Authentication.Jwt.Internals;

internal record ClaimRepositoryData<TIdentityClaim, TDecryptionKeys>
(
    TIdentityClaim IdentityClaim,
    DateTime ExpirationDate,
    Claim[] Claims
)
{
    [JsonIgnore]
    public TDecryptionKeys DecryptionKeys { get; set; } = default!;
}
