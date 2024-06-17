using Wkg.AspNetCore.Authentication.Claims;

namespace Wkg.AspNetCore.Authentication.Internals;

internal record ClaimRepositoryData<TIdentityClaim>(TIdentityClaim IdentityClaim, DateTime ExpirationDate, Claim[] Claims);
