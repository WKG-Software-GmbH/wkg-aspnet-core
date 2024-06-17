using System.Diagnostics.CodeAnalysis;
using Wkg.AspNetCore.Authentication.Claims;
using Wkg.AspNetCore.Authentication.Internals;

namespace Wkg.AspNetCore.Authentication;

public interface IClaimManager<TIdentityClaim, TExtendedKeys> : IClaimManager<TIdentityClaim>
    where TIdentityClaim : IdentityClaim
    where TExtendedKeys : IExtendedKeys<TExtendedKeys>
{
    new IClaimRepository<TIdentityClaim, TExtendedKeys> CreateRepository(TIdentityClaim identityClaim);

    internal bool TryDeserialize(string base64, [NotNullWhen(true)] out ClaimRepositoryData<TIdentityClaim, TExtendedKeys>? data);

    internal string Serialize(ClaimRepositoryData<TIdentityClaim, TExtendedKeys> scope);
}

public interface IClaimManager<TIdentityClaim> where TIdentityClaim : IdentityClaim
{
    IClaimRepository<TIdentityClaim> CreateRepository(TIdentityClaim identityClaim);

    bool TryRevokeClaims(TIdentityClaim identityClaim);

    IClaimValidationOptions Options { get; }

    bool TryRenewClaims(TIdentityClaim identityClaim);
}