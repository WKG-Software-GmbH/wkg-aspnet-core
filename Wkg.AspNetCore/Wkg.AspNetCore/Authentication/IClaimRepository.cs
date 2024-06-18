using System.Diagnostics.CodeAnalysis;
using Wkg.AspNetCore.Authentication.Claims;

namespace Wkg.AspNetCore.Authentication;

public interface IClaimRepository<TIdentityClaim, TExtendedKeys> : IClaimRepository<TIdentityClaim>
    where TIdentityClaim : IdentityClaim
    where TExtendedKeys : IExtendedKeys<TExtendedKeys>
{
    TExtendedKeys? ExtendedKeys { get; }

    new IClaimManager<TIdentityClaim, TExtendedKeys> ClaimManager { get; }

    [MemberNotNull(nameof(ExtendedKeys))]
    new void Initialize(TIdentityClaim identityClaim);
}

public interface IClaimRepository<TIdentityClaim> : ICollection<Claim>, IDisposable where TIdentityClaim : IdentityClaim
{
    ClaimRepositoryStatus Status { get; }

    IClaimManager<TIdentityClaim> ClaimManager { get; }

    DateTime ExpirationDate { get; set; }

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

    TIdentityClaim? IdentityClaim { get; }

    void Initialize(TIdentityClaim identityClaim);

    void Revoke();
}