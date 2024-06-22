using Wkg.AspNetCore.Authentication.Internals;

namespace Wkg.AspNetCore.Authentication.CookieBased;

internal record CookieClaimOptions(bool SecureOnly, ClaimValidationOptions ValidationOptions);