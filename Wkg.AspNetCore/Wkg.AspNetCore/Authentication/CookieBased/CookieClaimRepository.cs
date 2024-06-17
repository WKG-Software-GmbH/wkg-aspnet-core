using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Collections;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Wkg.AspNetCore.Authentication.Claims;
using Wkg.AspNetCore.Authentication.Internals;

namespace Wkg.AspNetCore.Authentication.CookieBased;

internal class CookieClaimRepository<TIdentityClaim> : IClaimRepository<TIdentityClaim> where TIdentityClaim : IdentityClaim
{
    private const string CookieName = "Wkg.AspNetCore.Authentication.CookieClaims";

    private readonly HttpContext _context;
    private readonly Dictionary<string, Claim> _claims;
    private bool _hasChanges;

    [ActivatorUtilitiesConstructor]
    public CookieClaimRepository(IHttpContextAccessor contextAccessor, IClaimManager<TIdentityClaim> claimManager)
    {
        _context = contextAccessor.HttpContext
            ?? throw new InvalidOperationException($"Failed to resolve {nameof(HttpContext)} from current repository.");
        ClaimManager = claimManager;
        if (_context.Request.Cookies.TryGetValue(CookieName, out string? cookieValue))
        {
            if (claimManager.TryDeserialize(cookieValue, out ClaimRepositoryData<TIdentityClaim>? data))
            {
                _claims = data.Claims.ToDictionary(c => c.Subject, c => c);
                _claims.Add(data.IdentityClaim.Subject, data.IdentityClaim);
                IdentityClaim = data.IdentityClaim;
                ExpirationDate = data.ExpirationDate;
                IsInitialized = true;
            }
            else
            {
                _context.Response.Cookies.Delete(CookieName);
                _claims = [];
            }
        }
        else
        {
            _claims = [];
        }
    }

    internal CookieClaimRepository(IHttpContextAccessor contextAccessor, IClaimManager<TIdentityClaim> claimManager, TIdentityClaim identityClaim, DateTime? expirationDate)
    {
        _context = contextAccessor.HttpContext
            ?? throw new InvalidOperationException($"Failed to resolve {nameof(HttpContext)} from current repository.");
        _claims = [];
        ClaimManager = claimManager;
        IdentityClaim = identityClaim;
        ExpirationDate = expirationDate;
        IsInitialized = true;
        _hasChanges = true;
    }

    public IClaimManager<TIdentityClaim> ClaimManager { get; }

    public TIdentityClaim? IdentityClaim { get; private set; }

    public DateTime? ExpirationDate { get; set; }

    [MemberNotNullWhen(true, nameof(IdentityClaim))]
    public bool IsInitialized { get; private set; }

    [MemberNotNullWhen(true, nameof(IdentityClaim))]
    public bool IsValid => IsInitialized && IdentityClaim is not null && (ExpirationDate is null || ExpirationDate > DateTime.UtcNow);

    public int Count => _claims.Count;

    public bool IsReadOnly => false;

    public void Initialize(TIdentityClaim identityClaim)
    {
        if (IsInitialized)
        {
            throw new InvalidOperationException("Repository is already initialized.");
        }
        IdentityClaim = identityClaim;
        ExpirationDate = ClaimManager.Options.TimeToLive.HasValue ? DateTime.UtcNow.Add(ClaimManager.Options.TimeToLive.Value) : null;
        IsInitialized = true;
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

    public void SetClaim<TValue>(Claim<TValue> claim) => this[claim.Subject] = claim;

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
        if (_hasChanges)
        {
            if (!IsInitialized)
            {
                throw new InvalidOperationException("Unable to serialize uninitialized repository.");
            }
            ClaimRepositoryData<TIdentityClaim> data = new
            (
                IdentityClaim,
                ExpirationDate,
                [
                    .. _claims.Values.Where(c => !ReferenceEquals(c, IdentityClaim))
                ]
            );
            string cookieValue = ClaimManager.Serialize(data);
            _context.Response.Cookies.Append(CookieName, cookieValue, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = ExpirationDate
            });
            _hasChanges = false;
            return true;
        }
        return false;
    }
}