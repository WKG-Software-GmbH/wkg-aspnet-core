using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Collections;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Wkg.AspNetCore.Authentication.Claims;
using Wkg.AspNetCore.Authentication.Jwt.Claims;
using Wkg.AspNetCore.Authentication.Jwt.Internals;
using Wkg.Logging;

namespace Wkg.AspNetCore.Authentication.Jwt.Implementations.CookieBased;

internal class CookieClaimRepository<TIdentityClaim, TDecryptionKeys> : IClaimRepository<TIdentityClaim, TDecryptionKeys>
    where TIdentityClaim : IdentityClaim
    where TDecryptionKeys : IDecryptionKeys<TDecryptionKeys>
{
    private const string CookieName = "Wkg.AspNetCore.Authentication.CookieClaims";

    private readonly HttpContext _context;
    private readonly Dictionary<string, Claim> _claims;
    private readonly CookieClaimOptions _cookieOptions;

    private bool _hasChanges;
    private bool _disposedValue;
    private DateTime _expirationDate;

    [ActivatorUtilitiesConstructor]
    public CookieClaimRepository(IHttpContextAccessor contextAccessor, IClaimManager<TIdentityClaim, TDecryptionKeys> claimManager, CookieClaimOptions cookieOptions)
    {
        _cookieOptions = cookieOptions;
        _context = contextAccessor.HttpContext
            ?? throw new InvalidOperationException($"Failed to resolve {nameof(HttpContext)} from current repository.");
        ClaimManager = claimManager;
        if (_context.Request.Cookies.TryGetValue(CookieName, out string? cookieValue))
        {
            if (claimManager.TryDeserialize(cookieValue, out ClaimRepositoryData<TIdentityClaim, TDecryptionKeys>? data, out ClaimRepositoryStatus status)
                || status is ClaimRepositoryStatus.Expired && data is not null)
            {
                _claims = data.Claims.ToDictionary(c => c.Subject, c => c);
                _claims.Add(data.IdentityClaim.Subject, data.IdentityClaim);
                IdentityClaim = data.IdentityClaim;
                ExpirationDate = data.ExpirationDate;
                DecryptionKeys = data.DecryptionKeys;
            }
            else
            {
                _context.Response.Cookies.Delete(CookieName);
                _claims = [];
            }
            Status = status;
        }
        else
        {
            Status = ClaimRepositoryStatus.Uninitialized;
            _claims = [];
        }
    }

    internal CookieClaimRepository(IHttpContextAccessor contextAccessor, IClaimManager<TIdentityClaim, TDecryptionKeys> claimManager, TIdentityClaim identityClaim, DateTime expirationDate, CookieClaimOptions cookieOptions)
    {
        _cookieOptions = cookieOptions;
        _context = contextAccessor.HttpContext
            ?? throw new InvalidOperationException($"Failed to resolve {nameof(HttpContext)} from current repository.");
        _claims = [];
        ClaimManager = claimManager;
        IdentityClaim = identityClaim;
        ExpirationDate = expirationDate;
        DecryptionKeys = TDecryptionKeys.Generate();
        _hasChanges = true;
        Status = ClaimRepositoryStatus.Valid;
    }

    public IClaimManager<TIdentityClaim, TDecryptionKeys> ClaimManager { get; }

    public TIdentityClaim? IdentityClaim { get; private set; }

    public TDecryptionKeys? DecryptionKeys { get; private set; }

    public ClaimRepositoryStatus Status { get; private set; }

    public DateTime ExpirationDate
    {
        get => _expirationDate;
        set
        {
            _expirationDate = value;
            _hasChanges = true;
        }
    }

    [MemberNotNullWhen(true, nameof(IdentityClaim), nameof(DecryptionKeys))]
    internal bool IsInitialized => Status is not ClaimRepositoryStatus.Uninitialized && IdentityClaim is not null && DecryptionKeys is not null;

    [MemberNotNullWhen(true, nameof(IdentityClaim))]
    public bool IsValid => IsInitialized && Status is ClaimRepositoryStatus.Valid && IdentityClaim is not null && ExpirationDate > DateTime.UtcNow;

    IClaimManager<TIdentityClaim> IClaimRepository<TIdentityClaim>.ClaimManager => ClaimManager;

    public int Count => _claims.Count;

    public bool IsReadOnly => false;

    public bool HasChanges => _hasChanges;

    public void Initialize(TIdentityClaim identityClaim)
    {
        IdentityClaim = identityClaim;
        ExpirationDate = DateTime.UtcNow.Add(ClaimManager.Options.TimeToLive);
        DecryptionKeys = TDecryptionKeys.Generate();
        Status = ClaimRepositoryStatus.Valid;
        _hasChanges = true;
    }

    public Claim this[string subject]
    {
        get => _claims[subject];
        set
        {
            _claims[subject] = value;
            _hasChanges = true;
        }
    }

    public void Add(Claim item)
    {
        _claims.Add(item.Subject, item);
        _hasChanges = true;
    }

    public IEnumerable<Claim<TValue>> Claims<TValue>() => _claims.Values.OfType<Claim<TValue>>();

    public void Clear()
    {
        _claims.Clear();
        _hasChanges = true;
    }

    public bool Contains(Claim item) => _claims.ContainsKey(item.Subject);

    public bool ContainsClaim(string subject) => _claims.ContainsKey(subject);

    public void CopyTo(Claim[] array, int arrayIndex) => _claims.Values.CopyTo(array, arrayIndex);

    private Claim<TValue> UpgradeToTyped<TValue>(Claim claim)
    {
        Claim<TValue>? typedClaim = claim.ToClaim<TValue>();
        if (!ReferenceEquals(claim, typedClaim))
        {
            _ = typedClaim ?? throw new InvalidCastException($"Failed to cast {claim.Subject} to {typeof(TValue)}.");
            Debug.Assert(claim.Subject == typedClaim.Subject);
            _claims[claim.Subject] = typedClaim;
        }
        return typedClaim;
    }

    public Claim<TValue> GetClaim<TValue>(string subject)
    {
        Claim claim = _claims[subject];
        return UpgradeToTyped<TValue>(claim);
    }

    public Claim<TValue>? GetClaimOrDefault<TValue>(string subject)
    {
        if (_claims.TryGetValue(subject, out Claim? claim))
        {
            return UpgradeToTyped<TValue>(claim);
        }
        return null;
    }

    public IEnumerator<Claim> GetEnumerator() => _claims.Values.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public bool Remove(Claim item)
    {
        bool removed = _claims.Remove(item.Subject);
        _hasChanges |= removed;
        return removed;
    }

    public void AddOrUpdate<TValue>(Claim<TValue> claim) => this[claim.Subject] = claim;

    public bool TryAddClaim<TValue>(Claim<TValue> claim)
    {
        if (_claims.TryAdd(claim.Subject, claim))
        {
            _hasChanges = true;
            return true;
        }
        return false;
    }

    public bool TryGetClaim<TValue>(string subject, [NotNullWhen(true)] out Claim<TValue>? claim)
    {
        if (_claims.TryGetValue(subject, out Claim? baseClaim))
        {
            claim = UpgradeToTyped<TValue>(baseClaim);
            return claim is not null;
        }
        claim = null;
        return false;
    }

    public bool SaveChanges()
    {
        ObjectDisposedException.ThrowIf(_disposedValue, this);
        if (_hasChanges)
        {
            if (!IsInitialized)
            {
                throw new InvalidOperationException("Unable to serialize uninitialized repository.");
            }
            ClaimRepositoryData<TIdentityClaim, TDecryptionKeys> data = new
            (
                IdentityClaim,
                ExpirationDate,
                [
                    .. _claims.Values.Where(c => !ReferenceEquals(c, IdentityClaim))
                ]
            )
            {
                DecryptionKeys = DecryptionKeys
            };
            ClaimManager.TryRenewClaims(IdentityClaim);
            string cookieValue = ClaimManager.Serialize(data);
            _context.Response.Cookies.Append(CookieName, cookieValue, new CookieOptions
            {
                HttpOnly = true,
                Secure = _cookieOptions.SecureOnly,
                SameSite = SameSiteMode.Strict,
                Expires = ExpirationDate
            });
            _hasChanges = false;
            return true;
        }
        return false;
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            _disposedValue = true;
        }
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    public void Revoke()
    {
        if (IsInitialized)
        {
            string oldIdentity = IdentityClaim.RawValue!;
            ClaimManager.TryRevokeClaims(IdentityClaim);
            _context.Response.Cookies.Delete(CookieName);
            _claims.Clear();
            IdentityClaim = null;
            DecryptionKeys = default;
            ExpirationDate = default;
            Status = ClaimRepositoryStatus.Uninitialized;
            _hasChanges = false;
            Log.WriteDebug($"[SECURITY] Claim repository for IdentityClaim {oldIdentity} has been revoked.");
        }
    }
}