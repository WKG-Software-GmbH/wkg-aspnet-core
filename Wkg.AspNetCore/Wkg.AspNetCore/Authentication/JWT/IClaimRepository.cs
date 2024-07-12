using System.Diagnostics.CodeAnalysis;
using Wkg.AspNetCore.Authentication.Claims;
using Wkg.AspNetCore.Authentication.Jwt.Claims;

namespace Wkg.AspNetCore.Authentication.Jwt;

/// <summary>
/// Represents a repository of claims that are associated with a specific <typeparamref name="TIdentityClaim"/> instance.
/// </summary>
/// <typeparam name="TIdentityClaim">The type of identity claim.</typeparam>
public interface IClaimRepository<TIdentityClaim> : ICollection<Claim>, IDisposable where TIdentityClaim : IdentityClaim
{
    /// <summary>
    /// The status of the repository.
    /// </summary>
    ClaimRepositoryStatus Status { get; }

    /// <summary>
    /// Retrieves the parent <see cref="IClaimManager{TIdentityClaim}"/> instance responsible for managing this repository.
    /// </summary>
    IClaimManager<TIdentityClaim> ClaimManager { get; }

    /// <summary>
    /// The UTC date and time when the validity of the claims in this repository expires.
    /// </summary>
    DateTime ExpirationDate { get; set; }

    /// <summary>
    /// Indicates whether the repository is fully initialized and contains valid, non-expired claims that may not have been persisted yet.
    /// </summary>
    [MemberNotNullWhen(true, nameof(IdentityClaim))]
    bool IsValid { get; }

    /// <summary>
    /// Indicates whether a subsequent call to <see cref="SaveChanges"/> would result in any changes being persisted.
    /// </summary>
    bool HasChanges { get; }

    /// <summary>
    /// Retrieves all typed claims from the repository with the specified type <typeparamref name="TValue"/>.
    /// </summary>
    IEnumerable<Claim<TValue>> Claims<TValue>();

    /// <summary>
    /// Attempts to retrieve a typed claim with the specified <paramref name="subject"/> and type <typeparamref name="TValue"/> from the repository.
    /// </summary>
    /// <param name="subject">The subject of the claim to retrieve.</param>
    /// <param name="claim">The claim that was retrieved, or <see langword="null"/> if the claim was not found or the conversion failed.</param>
    /// <typeparam name="TValue">The type of the claim value.</typeparam>
    /// <returns><see langword="true"/> if the claim was found and successfully retrieved; otherwise, <see langword="false"/>.</returns>
    bool TryGetClaim<TValue>(string subject, [NotNullWhen(true)] out Claim<TValue>? claim);

    /// <summary>
    /// Attempts to add a new claim to the repository.
    /// </summary>
    /// <typeparam name="TValue">The type of the claim value.</typeparam>
    /// <param name="claim">The claim to add.</param>
    /// <returns><see langword="true"/> if the claim was successfully added; otherwise, <see langword="false"/> if a claim with the same subject already exists.</returns>
    bool TryAddClaim<TValue>(Claim<TValue> claim);

    /// <summary>
    /// Retrieves a claim with the specified <paramref name="subject"/> and type <typeparamref name="TValue"/> from the repository or returns <see langword="null"/> if the claim was not found or the conversion is not possible.
    /// </summary>
    /// <typeparam name="TValue">The type of the claim value.</typeparam>
    /// <param name="subject">The subject of the claim to retrieve.</param>
    Claim<TValue>? GetClaimOrDefault<TValue>(string subject);

    /// <summary>
    /// Retrieves a claim with the specified <paramref name="subject"/> and type <typeparamref name="TValue"/> from the repository.
    /// </summary>
    /// <typeparam name="TValue">The type of the claim value.</typeparam>
    /// <param name="subject">The subject of the claim to retrieve.</param>
    /// <exception cref="KeyNotFoundException">if no claim with the specified <paramref name="subject"/> was found.</exception>
    /// <exception cref="InvalidCastException">if the claim with the specified <paramref name="subject"/> could not be converted to the specified type <typeparamref name="TValue"/>.</exception>
    Claim<TValue> GetClaim<TValue>(string subject);

    /// <summary>
    /// Adds or updates a claim in the repository.
    /// </summary>
    /// <typeparam name="TValue">The type of the claim value.</typeparam>
    /// <param name="claim">The claim to add or update.</param>
    void AddOrUpdate<TValue>(Claim<TValue> claim);

    /// <summary>
    /// Adds, updates, or retrieves a claim in the repository.
    /// </summary>
    /// <param name="subject">The subject of the claim.</param>
    Claim this[string subject] { get; set; }

    /// <summary>
    /// Indicates whether the repository contains a claim with the specified <paramref name="subject"/>.
    /// </summary>
    /// <param name="subject">The subject of the claim to check for.</param>
    /// <returns><see langword="true"/> if a corresponding claim was found; otherwise, <see langword="false"/>.</returns>
    bool ContainsClaim(string subject);

    /// <summary>
    /// Persists any outstanding changes to the repository.
    /// </summary>
    /// <returns><see langword="true"/> if state changes were written to the underlying storage; otherwise, <see langword="false"/> if no changes required persistence.</returns>
    bool SaveChanges();

    /// <summary>
    /// The identity claim associated with this repository, or <see langword="null"/> if the repository is not yet been initialized.
    /// </summary>
    TIdentityClaim? IdentityClaim { get; }

    /// <summary>
    /// Initializes the repository with the specified <paramref name="identityClaim"/>.
    /// </summary>
    /// <param name="identityClaim">The identity claim to associate with this repository.</param>
    void Initialize(TIdentityClaim identityClaim);

    /// <summary>
    /// Invalidates the underlying session, immediately revokes all claims associated with the repository, and persists the changes, leaving this repository in an uninitialized state.
    /// </summary>
    void Revoke();
}

/// <summary>
/// Represents a repository of claims that are associated with a specific <typeparamref name="TIdentityClaim"/> instance with support for decryption keys.
/// </summary>
/// <typeparam name="TIdentityClaim">The type of identity claim.</typeparam>
/// <typeparam name="TDecryptionKeys">The type of decryption key data.</typeparam>
public interface IClaimRepository<TIdentityClaim, TDecryptionKeys> : IClaimRepository<TIdentityClaim>
    where TIdentityClaim : IdentityClaim
    where TDecryptionKeys : IDecryptionKeys<TDecryptionKeys>
{
    /// <summary>
    /// The decryption key data associated with this repository, or <see langword="null"/> if the repository is not yet been initialized.
    /// </summary>
    TDecryptionKeys? DecryptionKeys { get; }

    /// <summary>
    /// Retrieves the parent <see cref="IClaimManager{TIdentityClaim, TDecryptionKeys}"/> instance responsible for managing this repository.
    /// </summary>
    new IClaimManager<TIdentityClaim, TDecryptionKeys> ClaimManager { get; }

    /// <inheritdoc cref="IClaimRepository{TIdentityClaim}.Initialize(TIdentityClaim)"/>
    [MemberNotNull(nameof(DecryptionKeys))]
    new void Initialize(TIdentityClaim identityClaim);
}