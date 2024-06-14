using Microsoft.AspNetCore.Http;
using System.Buffers;
using System.Buffers.Text;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Wkg.Data.Pooling;
using Wkg.Logging;

namespace Wkg.AspNetCore.Authentication;

public interface IClaimManagerScope : ICollection<Claim>
{
    DateTime? ExpirationDate { get; set; }

    bool IsValid { get; }

    bool IsInitialized { get; }

    IEnumerable<Claim<TValue>> Claims<TValue>();

    bool TryGetClaim<TValue>(string subject, [NotNullWhen(true)] out Claim<TValue>? claim);

    bool TryAddClaim<TValue>(Claim<TValue> claim);

    Claim<TValue>? GetClaimOrDefault<TValue>(string subject);

    Claim<TValue> GetClaim<TValue>(string subject);

    void SetClaim<TValue>(Claim<TValue> claim);

    Claim this[string subject] { get; set; }

    bool ContainsClaim(string subject);

    bool SaveChanges();
}

public interface IClaimManagerScope<TIdentityClaim> : IClaimManagerScope
    where TIdentityClaim : IdentityClaim
{
    IClaimManager<TIdentityClaim> ClaimManager { get; }

    TIdentityClaim? IdentityClaim { get; }
}

public interface IClaimManager<TIdentityClaim> where TIdentityClaim : IdentityClaim
{
    IClaimManagerScope<TIdentityClaim> CreateScope(TIdentityClaim identityClaim);

    internal bool TryDeserialize(string base64, [NotNullWhen(true)] out ClaimScopeData<TIdentityClaim>? data);

    internal string Serialize(ClaimScopeData<TIdentityClaim> scope);

    bool TryRevokeClaims(TIdentityClaim identityClaim);
}

internal class CookieClaimManager<TIdentityClaim>(IHttpContextAccessor contextAccessor, ClaimValidationOptions options, SessionKeyStore session) : IClaimManager<TIdentityClaim> where TIdentityClaim : IdentityClaim
{
    public IClaimManagerScope<TIdentityClaim> CreateScope(TIdentityClaim identityClaim)
    {
        CookieClaimManagerScope<TIdentityClaim> scope = new(contextAccessor, this, identityClaim, options.Expiration.HasValue ? DateTime.UtcNow.Add(options.Expiration.Value) : null);
        return scope;
    }

    string IClaimManager<TIdentityClaim>.Serialize(ClaimScopeData<TIdentityClaim> scope)
    {
        using MemoryStream stream = new();
        JsonSerializer.Serialize(stream, scope);
    RETRY:
        if (!session.SessionKeys.TryGetValue(scope.IdentityClaim.RawValue, out Guid sessionKey))
        {
            sessionKey = Guid.NewGuid();
            if (!session.SessionKeys.TryAdd(scope.IdentityClaim.RawValue, sessionKey))
            {
                goto RETRY;
            }
        }
        Span<byte> sessionKeyBytes = stackalloc byte[16];
        if (!sessionKey.TryWriteBytes(sessionKeyBytes)) 
        {
            throw new InvalidOperationException("Failed to write session key bytes.");
        }
        PooledArray<byte> keyBuffer = ArrayPool.Rent<byte>(options.SecretBytes.Length + 16);
        Span<byte> keyBufferSpan = keyBuffer.AsSpan();
        Unsafe.CopyBlock(ref keyBufferSpan[0], in options.SecretBytes[0], (uint)options.SecretBytes.Length);
        Unsafe.CopyBlock(ref keyBufferSpan[options.SecretBytes.Length], in sessionKeyBytes[0], (uint)sessionKeyBytes.Length);
        Span<byte> hmac = stackalloc byte[HMACSHA512.HashSizeInBytes];
        stream.Position = 0;
        int bytesWritten = HMACSHA512.HashData(keyBufferSpan, stream, hmac);
        keyBufferSpan.Clear();
        Debug.Assert(bytesWritten == HMACSHA512.HashSizeInBytes);
        ArrayPool.Return(keyBuffer);
        stream.Write(hmac);
        PooledArray<byte> result = ArrayPool.Rent<byte>((int)stream.Length);
        Span<byte> resultSpan = result.AsSpan();
        stream.Position = 0;
        stream.Read(resultSpan);
        string base64 = Convert.ToBase64String(resultSpan);
        ArrayPool.Return(result);
        return base64;
    }

    bool IClaimManager<TIdentityClaim>.TryDeserialize(string base64, [NotNullWhen(true)] out ClaimScopeData<TIdentityClaim>? data)
    {
        PooledArray<byte> buffer = ArrayPool.Rent<byte>(base64.Length);
        Span<byte> bufferSpan = buffer.AsSpan();
        int bytesWritten = Encoding.UTF8.GetBytes(base64, bufferSpan);
        if (bytesWritten != buffer.Length)
        {
            Log.WriteWarning("Failed to decode base64 string. The provided string contains non-ASCII characters.");
            ArrayPool.Return(buffer);
            data = null;
            return false;
        }
        OperationStatus status = Base64.DecodeFromUtf8InPlace(bufferSpan, out bytesWritten);
        if (status != OperationStatus.Done || bytesWritten <= HMACSHA512.HashSizeInBytes)
        {
            Log.WriteWarning("Failed to decode base64 string. The provided string is not a valid base64 string.");
            ArrayPool.Return(buffer);
            data = null;
            return false;
        }
        Span<byte> decodedBytes = bufferSpan[..bytesWritten];
        Span<byte> hmac = decodedBytes[^HMACSHA512.HashSizeInBytes..];
        Span<byte> content = decodedBytes[..^HMACSHA512.HashSizeInBytes];
        data = JsonSerializer.Deserialize<ClaimScopeData<TIdentityClaim>>(content);
        if (data is null)
        {
            Log.WriteWarning("Failed to deserialize claim scope data.");
            ArrayPool.Return(buffer);
            return false;
        }
        if (!session.SessionKeys.TryGetValue(data.IdentityClaim.RawValue, out Guid sessionKey))
        {
            Log.WriteWarning("Invalid or expired session key.");
            ArrayPool.Return(buffer);
            return false;
        }
        Span<byte> sessionKeyBytes = stackalloc byte[16];
        if (!sessionKey.TryWriteBytes(sessionKeyBytes))
        {
            Log.WriteWarning("Invalid or expired session key.");
            ArrayPool.Return(buffer);
            return false;
        }
        PooledArray<byte> keyBuffer = ArrayPool.Rent<byte>(options.SecretBytes.Length + 16);
        Span<byte> keyBufferSpan = keyBuffer.AsSpan();
        Unsafe.CopyBlock(ref keyBufferSpan[0], in options.SecretBytes[0], (uint)options.SecretBytes.Length);
        Unsafe.CopyBlock(ref keyBufferSpan[options.SecretBytes.Length], in sessionKeyBytes[0], (uint)sessionKeyBytes.Length);
        Span<byte> computedHmac = stackalloc byte[HMACSHA512.HashSizeInBytes];
        bytesWritten = HMACSHA512.HashData(keyBufferSpan, content, computedHmac);
        keyBufferSpan.Clear();
        Debug.Assert(bytesWritten == HMACSHA512.HashSizeInBytes);
        ArrayPool.Return(keyBuffer);
        ArrayPool.Return(buffer);
        if (!computedHmac.SequenceEqual(hmac))
        {
            Log.WriteError($"[SECURITY] Session key HMAC mismatch. This may indicate an attempt to tamper with the session data for IdentityClaim {data.IdentityClaim.RawValue}.");
            return false;
        }
        if (data.ExpirationDate.HasValue && data.ExpirationDate.Value < DateTime.UtcNow)
        {
            Log.WriteWarning($"Session key for IdentityClaim {data.IdentityClaim.RawValue} has expired.");
            return false;
        }
        Log.WriteDebug($"Audit success: Session key for IdentityClaim {data.IdentityClaim.RawValue} has been validated.");
        return true;
    }

    public bool TryRevokeClaims(TIdentityClaim identityClaim) => session.SessionKeys.TryRemove(identityClaim.Subject, out _);
}

internal record ClaimValidationOptions(string Secret, TimeSpan? Expiration = null)
{
    public byte[] SecretBytes { get; } = Encoding.UTF8.GetBytes(Secret);
}

internal record ClaimScopeData<TIdentityClaim>(TIdentityClaim IdentityClaim, DateTime? ExpirationDate, Claim[] Claims);

internal record SessionKeyStore()
{
    public ConcurrentDictionary<string, Guid> SessionKeys { get; } = [];
}