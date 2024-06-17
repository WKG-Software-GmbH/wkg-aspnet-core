using System.Diagnostics.CodeAnalysis;
using Wkg.AspNetCore.Authentication.Claims;

namespace Wkg.AspNetCore.Authentication;

public interface IClaimRepository<TIdentityClaim> : ICollection<Claim>
    where TIdentityClaim : IdentityClaim
{
    DateTime? ExpirationDate { get; set; }

    [MemberNotNullWhen(true, nameof(IdentityClaim))]
    bool IsValid { get; }

    [MemberNotNullWhen(true, nameof(IdentityClaim))]
    bool IsInitialized { get; }

    IEnumerable<Claim<TValue>> Claims<TValue>();

    bool TryGetClaim<TValue>(string subject, [NotNullWhen(true)] out Claim<TValue>? claim);

    bool TryAddClaim<TValue>(Claim<TValue> claim);

    Claim<TValue>? GetClaimOrDefault<TValue>(string subject);

    Claim<TValue> GetClaim<TValue>(string subject);

    void SetClaim<TValue>(Claim<TValue> claim);

    Claim this[string subject] { get; set; }

    bool ContainsClaim(string subject);

    bool SaveChanges();

    IClaimManager<TIdentityClaim> ClaimManager { get; }

    TIdentityClaim? IdentityClaim { get; }

    void Initialize(TIdentityClaim identityClaim);
}
