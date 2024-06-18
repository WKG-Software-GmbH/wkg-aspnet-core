using System.Diagnostics.CodeAnalysis;
using Wkg.AspNetCore.Authentication.Claims;
using Wkg.AspNetCore.Authentication.Internals;

namespace Wkg.AspNetCore.Authentication;

/// <summary>
/// Represents a central authority for creating and revoking <see cref="IClaimRepository{TIdentityClaim}"/> instances.
/// </summary>
/// <typeparam name="TIdentityClaim">The type of identity claim.</typeparam>
public interface IClaimManager<TIdentityClaim> where TIdentityClaim : IdentityClaim
{
    /// <summary>
    /// Creates a new <see cref="IClaimRepository{TIdentityClaim}"/> instance for the specified <paramref name="identityClaim"/>.
    /// </summary>
    /// <param name="identityClaim">The identity claim of the repository.</param>
    IClaimRepository<TIdentityClaim> CreateRepository(TIdentityClaim identityClaim);

    /// <summary>
    /// Attempts to invalidate the session and revoke all claims associated with the specified <paramref name="identityClaim"/>.
    /// </summary>
    /// <param name="identityClaim">The identity claim to revoke.</param>
    /// <returns><see langword="true"/> if the session was found and successfully revoked; otherwise, <see langword="false"/> if the session wasn't valid to begin with.</returns>
    bool TryRevokeClaims(TIdentityClaim identityClaim);

    /// <summary>
    /// Retrieves the settings for claim validation.
    /// </summary>
    IClaimValidationOptions Options { get; }

    /// <summary>
    /// Attempts to reset the expiry date of the claims associated with the specified <paramref name="identityClaim"/>.
    /// </summary>
    /// <param name="identityClaim">The identity claim of the repository to renew.</param>
    /// <returns><see langword="true"/> if the session was found and successfully renewed; otherwise, <see langword="false"/></returns>
    bool TryRenewClaims(TIdentityClaim identityClaim);
}

/// <summary>
/// Represents a central authority for creating and revoking <see cref="IClaimRepository{TIdentityClaim, TExtendedKeys}"/> instances with extended key data.
/// </summary>
/// <typeparam name="TIdentityClaim">The type of identity claim.</typeparam>
/// <typeparam name="TExtendedKeys">The type of extended keys.</typeparam>
public interface IClaimManager<TIdentityClaim, TExtendedKeys> : IClaimManager<TIdentityClaim>
    where TIdentityClaim : IdentityClaim
    where TExtendedKeys : IExtendedKeys<TExtendedKeys>
{
    /// <summary>
    /// Creates a new <see cref="IClaimRepository{TIdentityClaim, TExtendedKeys}"/> instance for the specified <paramref name="identityClaim"/>.
    /// </summary>
    /// <param name="identityClaim">The identity claim of the repository.</param>
    /// <returns>A new instance of <see cref="IClaimRepository{TIdentityClaim, TExtendedKeys}"/>.</returns>
    new IClaimRepository<TIdentityClaim, TExtendedKeys> CreateRepository(TIdentityClaim identityClaim);

    internal bool TryDeserialize(string base64, [NotNullWhen(true)] out ClaimRepositoryData<TIdentityClaim, TExtendedKeys>? data, out ClaimRepositoryStatus status);

    internal string Serialize(ClaimRepositoryData<TIdentityClaim, TExtendedKeys> scope);
}