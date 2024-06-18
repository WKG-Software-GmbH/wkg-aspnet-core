using Microsoft.AspNetCore.Http;
using System.Buffers;
using System.Buffers.Text;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Wkg.AspNetCore.Authentication.Claims;
using Wkg.AspNetCore.Authentication.Internals;
using Wkg.Data.Pooling;
using Wkg.Logging;

namespace Wkg.AspNetCore.Authentication.CookieBased;

internal class CookieClaimManager<TIdentityClaim, TDecryptionKeys>(IHttpContextAccessor contextAccessor, ClaimValidationOptions options, SessionKeyStore<TDecryptionKeys> sessions)
    : IClaimManager<TIdentityClaim, TDecryptionKeys>
    where TIdentityClaim : IdentityClaim 
    where TDecryptionKeys : IDecryptionKeys<TDecryptionKeys>
{
    public IClaimRepository<TIdentityClaim, TDecryptionKeys> CreateRepository(TIdentityClaim identityClaim)
    {
        CookieClaimRepository<TIdentityClaim, TDecryptionKeys> scope = new(contextAccessor, this, identityClaim, DateTime.UtcNow.Add(options.TimeToLive));
        return scope;
    }

    IClaimRepository<TIdentityClaim> IClaimManager<TIdentityClaim>.CreateRepository(TIdentityClaim identityClaim) => CreateRepository(identityClaim);

    public IClaimValidationOptions Options => options;

    string IClaimManager<TIdentityClaim, TDecryptionKeys>.Serialize(ClaimRepositoryData<TIdentityClaim, TDecryptionKeys> repository)
    {
        ArgumentNullException.ThrowIfNull(repository.IdentityClaim.RawValue, nameof(repository.IdentityClaim.RawValue));
        foreach (Claim claim in repository.Claims)
        {
            if (claim.RequiresSerialization)
            {
                claim.Serialize();
            }
        }
        using MemoryStream stream = new();
        // write JSON data to stream
        JsonSerializer.Serialize(stream, repository);
        // retrieve session key
        SessionKey<TDecryptionKeys> sessionKey = sessions.GetOrCreateSession(repository.IdentityClaim.RawValue, repository.DecryptionKeys);
        // allocate buffer for HMAC key data (server secret + session key)
        PooledArray<byte> keyBuffer = ArrayPool.Rent<byte>(options.SecretBytes.Length + sessionKey.Size);
        Span<byte> keyBufferSpan = keyBuffer.AsSpan();
        // copy server secret and session key into buffer
        Unsafe.CopyBlock(ref keyBufferSpan[0], in options.SecretBytes[0], (uint)options.SecretBytes.Length);
        // we can directly write the session key into the correct position in the buffer
        sessionKey.WriteKey(keyBufferSpan[^sessionKey.Size..]);
        // allocate result buffer for HMAC
        Span<byte> hmac = stackalloc byte[HMACSHA512.HashSizeInBytes];
        // don't forget to reset the Stream position to avoid computing the HMAC over 0 bytes
        stream.Position = 0;
        // compute Hash-based Message Authentication Code (HMAC) using server secret and session key
        int bytesWritten = HMACSHA512.HashData(keyBufferSpan, stream, hmac);
        Debug.Assert(bytesWritten == HMACSHA512.HashSizeInBytes);
        // we somewhat want to protect the key data from being exposed in memory, so at least clear these temporary copies of it
        // the buffer can be reused for the next HMAC computation
        ArrayPool.Return(keyBuffer, clearArray: true);
        // append the HMAC to the end of the stream
        stream.Write(hmac);
        // convert the stream to a base64 string using another pooled buffer large enough to hold the entire stream
        PooledArray<byte> result = ArrayPool.Rent<byte>((int)stream.Length);
        Span<byte> resultSpan = result.AsSpan();
        stream.Position = 0;
        stream.Read(resultSpan);
        string base64 = Convert.ToBase64String(resultSpan);
        // return the base64 string and release the buffer
        ArrayPool.Return(result);
        return base64;
    }

    bool IClaimManager<TIdentityClaim, TDecryptionKeys>.TryDeserialize(string base64, [NotNullWhen(true)] out ClaimRepositoryData<TIdentityClaim, TDecryptionKeys>? data, out ClaimRepositoryStatus status)
    {
        // deserialization requires more checks due to untrusted input
        // we assume that the base64 string is valid ASCII/UTF-8
        PooledArray<byte> buffer = ArrayPool.Rent<byte>(base64.Length);
        Span<byte> bufferSpan = buffer.AsSpan();
        int bytesWritten = Encoding.UTF8.GetBytes(base64, bufferSpan);
        // but we still need to check that that's the case
        if (bytesWritten != buffer.Length)
        {
            Log.WriteWarning("[SECURITY] Failed to decode base64 string. The provided string contains non-ASCII characters.");
            ArrayPool.Return(buffer);
            data = null;
            status = ClaimRepositoryStatus.Invalid;
            return false;
        }
        // in-place base64 decoding is fine because it will deflate the buffer size
        OperationStatus base64Status = Base64.DecodeFromUtf8InPlace(bufferSpan, out bytesWritten);
        // check that the base64 was valid and that the decoded data is at least as long as the HMAC (otherwise it's invalid)
        if (base64Status != OperationStatus.Done || bytesWritten <= HMACSHA512.HashSizeInBytes)
        {
            Log.WriteWarning("[SECURITY] Failed to decode base64 string. The provided string is not a valid base64 string.");
            ArrayPool.Return(buffer);
            data = null;
            status = ClaimRepositoryStatus.Invalid;
            return false;
        }
        // slice the buffer to the actual decoded data...
        Span<byte> decodedBytes = bufferSpan[..bytesWritten];
        // ... which is the content and the HMAC at the end
        // this should always be safe because we've already checked that the decoded data is at least as long as the HMAC
        Span<byte> hmac = decodedBytes[^HMACSHA512.HashSizeInBytes..];
        Span<byte> content = decodedBytes[..^HMACSHA512.HashSizeInBytes];
        // attempt to deserialize what we have into a ClaimRepositoryData object
        data = JsonSerializer.Deserialize<ClaimRepositoryData<TIdentityClaim, TDecryptionKeys>>(content);
        if (data?.IdentityClaim is null)
        {
            Log.WriteWarning("[SECURITY] Failed to deserialize claim scope data.");
            ArrayPool.Return(buffer);
            status = ClaimRepositoryStatus.Invalid;
            return false;
        }
        // the IdentityClaim.RawValue should never be null, but it might be if someone screws up their JSON serialization or the user-supplied data is invalid (HMAC mismatch)
        if (data.IdentityClaim?.RawValue is null)
        {
            Log.WriteError("[SECURITY] IdentityClaim.RawValue is null. Are you sure JSON serialization is working correctly? Rejecting invalid claim scope data.");
            ArrayPool.Return(buffer);
            status = ClaimRepositoryStatus.Invalid;
            return false;
        }
        // attempt to retrieve the in-memory session key for the IdentityClaim
        if (!sessions.TryGetSession(data.IdentityClaim.RawValue, out SessionKey<TDecryptionKeys>? sessionKey))
        {
            Log.WriteWarning($"[SECURITY] Invalid or expired session key: {data.IdentityClaim.RawValue}.");
            ArrayPool.Return(buffer);
            // although the data could not be validated, we still return it to the caller (data is not null)
            // they might be able to re-authenticate the user and recover the session through whatever context they have from the claim data
            status = ClaimRepositoryStatus.Expired;
            return false;
        }
        // now validate the HMAC
        // rent a buffer large enough to hold the server secret and the session key
        PooledArray<byte> keyBuffer = ArrayPool.Rent<byte>(options.SecretBytes.Length + sessionKey.Size);
        // copy the server secret into the buffer
        Span<byte> keyBufferSpan = keyBuffer.AsSpan();
        Unsafe.CopyBlock(ref keyBufferSpan[0], in options.SecretBytes[0], (uint)options.SecretBytes.Length);
        // and write the session key into the correct position in the buffer
        sessionKey.WriteKey(keyBufferSpan[^sessionKey.Size..]);
        // the computed HMAC will fit on the stack
        Span<byte> computedHmac = stackalloc byte[HMACSHA512.HashSizeInBytes];
        // compute the HMAC using the server secret and the session key
        bytesWritten = HMACSHA512.HashData(keyBufferSpan, content, computedHmac);
        Debug.Assert(bytesWritten == HMACSHA512.HashSizeInBytes);
        // try to reduce the risk of leaking the key data to other parts of the application by clearing the buffer
        ArrayPool.Return(keyBuffer, clearArray: true);
        ArrayPool.Return(buffer);
        // compare the computed HMAC with the one from the base64 string
        if (!computedHmac.SequenceEqual(hmac))
        {
            Log.WriteError($"[SECURITY] Session key HMAC mismatch. This may indicate an attempt to tamper with the session data for IdentityClaim {data.IdentityClaim.RawValue}.");
            status = ClaimRepositoryStatus.Invalid;
            return false;
        }
        // validate the expiration date and the claims
        if (data.ExpirationDate < DateTime.UtcNow)
        {
            Log.WriteWarning($"[SECURITY] Session key for IdentityClaim {data.IdentityClaim.RawValue} has expired.");
            sessions.TryRevokeSession(data.IdentityClaim.RawValue);
            status = ClaimRepositoryStatus.Expired;
            return false;
        }
        // NULL-values in the claims are not allowed and are an indication of previously failed JSON serialization (tampering would be detected by the HMAC check)
        if (data.Claims.Any(c => c.RawValue is null))
        {
            Log.WriteWarning($"[SECURITY] One or more claims in the session key for IdentityClaim {data.IdentityClaim.RawValue} are null. Is your JSON serialization working correctly? Rejecting invalid claim scope data.");
            status = ClaimRepositoryStatus.Invalid;
            return false;
        }
        // yay :)
        Log.WriteDiagnostic($"[SECURITY] Audit success: Session key for IdentityClaim {data.IdentityClaim.RawValue} has been validated.");
        // don't forget to include the decryption keys in our response for user-code to apply custom decryption of protected claims
        data.DecryptionKeys = sessionKey.DecryptionKeys;
        status = ClaimRepositoryStatus.Valid;
        return true;
    }

    public bool TryRevokeClaims(TIdentityClaim identityClaim)
    {
        ArgumentNullException.ThrowIfNull(identityClaim.RawValue, nameof(identityClaim.RawValue));
        return sessions.TryRevokeSession(identityClaim.RawValue);
    }

    public bool TryRenewClaims(TIdentityClaim identityClaim)
    {
        if (!sessions.TryGetSession(identityClaim.Subject, out SessionKey<TDecryptionKeys>? sessionKey))
        {
            return false;
        }
        sessionKey.CreatedAt = Interlocked.Exchange(ref sessionKey.CreatedAt, DateTime.UtcNow.Ticks);
        return true;
    }
}
