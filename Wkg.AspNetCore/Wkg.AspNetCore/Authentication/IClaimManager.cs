using System.Diagnostics.CodeAnalysis;
using Wkg.AspNetCore.Authentication.Claims;
using Wkg.AspNetCore.Authentication.Internals;

namespace Wkg.AspNetCore.Authentication;

public interface IClaimManager<TIdentityClaim> where TIdentityClaim : IdentityClaim
{
    IClaimRepository<TIdentityClaim> CreateRepository(TIdentityClaim identityClaim);

    bool TryRevokeClaims(TIdentityClaim identityClaim);

    IClaimValidationOptions Options { get; }

    internal bool TryDeserialize(string base64, [NotNullWhen(true)] out ClaimRepositoryData<TIdentityClaim>? data);

    internal string Serialize(ClaimRepositoryData<TIdentityClaim> scope);

    bool TryRenewClaims(TIdentityClaim identityClaim);
}
