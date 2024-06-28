using System.Text.Json.Serialization;
using Wkg.AspNetCore.Authentication.Claims;

namespace Wkg.AspNetCore.Authentication.Internals;

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
