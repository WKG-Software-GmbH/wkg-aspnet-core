using System.Diagnostics.CodeAnalysis;
using Wkg.AspNetCore.Authentication.Claims;
using Wkg.AspNetCore.Authentication.Internals;

namespace Wkg.AspNetCore.Authentication;

public interface IClaimManager<TIdentityClaim> where TIdentityClaim : IdentityClaim
{
    IClaimRepository<TIdentityClaim> CreateRepository(TIdentityClaim identityClaim);

    bool TryRevokeClaims(TIdentityClaim identityClaim);

    internal bool TryDeserialize(string base64, [NotNullWhen(true)] out ClaimScopeData<TIdentityClaim>? data);

    internal string Serialize(ClaimScopeData<TIdentityClaim> scope);

    internal ClaimValidationOptions Options { get; }
}
