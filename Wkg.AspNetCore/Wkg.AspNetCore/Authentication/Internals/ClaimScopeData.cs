namespace Wkg.AspNetCore.Authentication.Internals;

internal record ClaimScopeData<TIdentityClaim>(TIdentityClaim IdentityClaim, DateTime? ExpirationDate, Claim[] Claims);
