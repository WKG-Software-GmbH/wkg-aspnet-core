using Wkg.AspNetCore.Authentication.Claims;

namespace Wkg.AspNetCore.Authentication;

/// <summary>
/// Represents the status of an <see cref="IClaimRepository{TIdentityClaim}"/>.
/// </summary>
public enum ClaimRepositoryStatus
{
    /// <summary>
    /// This repository has not yet been initialized and no <see cref="IdentityClaim"/> has been made.
    /// </summary>
    Uninitialized,

    /// <summary>
    /// This repository has been initialized and contains a valid <see cref="IdentityClaim"/> and may contain other claims.
    /// </summary>
    Valid,

    /// <summary>
    /// An attempt was made to deserialize the repository, but the data was invalid or could not be verified.
    /// </summary>
    Invalid,

    /// <summary>
    /// The repository was successfully deserialized, but all claims have expired and could not be verified. 
    /// This repository contains untrusted claims. 
    /// Proceed with caution.
    /// </summary>
    Expired,
}