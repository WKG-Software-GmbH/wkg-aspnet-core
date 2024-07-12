using Microsoft.AspNetCore.Http;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json;
using Wkg.AspNetCore.Authentication.Claims;
using Wkg.AspNetCore.Authentication.Jwt.Claims;
using Wkg.AspNetCore.Authentication.Jwt.Internals;
using Wkg.AspNetCore.Authentication.Jwt.SigningFunctions.Implementations;
using Wkg.Data.Pooling;
using Wkg.Logging;

namespace Wkg.AspNetCore.Authentication.Jwt.Implementations.CookieBased;

internal class CookieClaimManager<TIdentityClaim, TDecryptionKeys>(IHttpContextAccessor contextAccessor, CookieClaimOptions cookieOptions, SessionKeyStore<TDecryptionKeys> sessions, IJwtSigningFunction signingFunction)
    : IClaimManager<TIdentityClaim, TDecryptionKeys>
    where TIdentityClaim : IdentityClaim
    where TDecryptionKeys : IDecryptionKeys<TDecryptionKeys>
{
    public IClaimRepository<TIdentityClaim, TDecryptionKeys> CreateRepository(TIdentityClaim identityClaim)
    {
        CookieClaimRepository<TIdentityClaim, TDecryptionKeys> scope = new(contextAccessor, this, identityClaim, DateTime.UtcNow.Add(cookieOptions.ValidationOptions.TimeToLive), cookieOptions);
        return scope;
    }

    IClaimRepository<TIdentityClaim> IClaimManager<TIdentityClaim>.CreateRepository(TIdentityClaim identityClaim) => CreateRepository(identityClaim);

    public IClaimValidationOptions Options => cookieOptions.ValidationOptions;

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
        ClaimValidationOptions options = cookieOptions.ValidationOptions;
        JwtHeader header = new(signingFunction.Name);
        using MemoryStream stream = new();

        // write JWT header to stream
        JsonSerializer.Serialize(stream, header);
        long headerLength = stream.Position;
        // write JSON data to stream
        JsonSerializer.Serialize(stream, repository);
        long payloadLength = stream.Position - headerLength;
        // allocate buffers for base64 url encoding
        PooledArray<byte> headerBuffer = ArrayPool.Rent<byte>((int)headerLength);
        PooledArray<byte> payloadBuffer = ArrayPool.Rent<byte>((int)payloadLength);
        Span<byte> headerSpan = headerBuffer.AsSpan();
        Span<byte> payloadSpan = payloadBuffer.AsSpan();
        // copy header data into buffer
        stream.Position = 0;
        int bytesRead = stream.Read(headerSpan);
        Debug.Assert(bytesRead == headerLength);
        // copy payload data into buffer
        bytesRead = stream.Read(payloadSpan);
        Debug.Assert(bytesRead == payloadLength);
        // encode header and payload to base64
        PooledArray<byte> headerBase64 = Base64UrlEncoder.Base64UrlEncode(headerSpan);
        PooledArray<byte> payloadBase64 = Base64UrlEncoder.Base64UrlEncode(payloadSpan);
        // write base64 encoded header and payload back to stream
        // this is fine, because the base64 encoding will always inflate the buffer size, so we overwrite all previous data
        stream.Position = 0;
        stream.Write(headerBase64.AsSpan());
        stream.WriteByte((byte)'.');
        stream.Write(payloadBase64.AsSpan());
        // retrieve session key
        SessionKey<TDecryptionKeys> sessionKey = sessions.GetOrCreateSession(repository.IdentityClaim.RawValue, repository.DecryptionKeys);
        // allocate result buffer for HMAC
        Span<byte> signature = stackalloc byte[signingFunction.SignatureLengthInBytes];
        // compute Hash-based Message Authentication Code (HMAC) using server secret and session key
        int bytesWritten = signingFunction.Sign(sessionKey, stream, signature);
        Debug.Assert(bytesWritten == signingFunction.SignatureLengthInBytes);
        // base64 url encode the HMAC
        PooledArray<byte> hmacBase64 = Base64UrlEncoder.Base64UrlEncode(signature);
        // convert the stream to a base64 string using another pooled buffer large enough to hold the entire stream
        PooledArray<byte> result = ArrayPool.Rent<byte>((int)stream.Length + hmacBase64.Length + 1);
        Span<byte> resultSpan = result.AsSpan();
        stream.Position = 0;
        bytesRead = stream.Read(resultSpan);
        Debug.Assert(bytesRead == stream.Length);
        resultSpan[bytesRead] = (byte)'.';
        Span<byte> hmacBase64Span = hmacBase64.AsSpan();
        hmacBase64Span.CopyTo(resultSpan[(bytesRead + 1)..]);
        string jwt = Encoding.ASCII.GetString(resultSpan);
        // return all buffers to the pool
        ArrayPool.Return(result);
        ArrayPool.Return(hmacBase64);
        ArrayPool.Return(payloadBase64);
        ArrayPool.Return(headerBase64);
        ArrayPool.Return(payloadBuffer);
        ArrayPool.Return(headerBuffer);
        return jwt;
    }

    bool IClaimManager<TIdentityClaim, TDecryptionKeys>.TryDeserialize(string base64, [NotNullWhen(true)] out ClaimRepositoryData<TIdentityClaim, TDecryptionKeys>? data, out ClaimRepositoryStatus status)
    {
        data = null;
        // deserialization requires more checks due to untrusted input
        // we assume that the base64 string is valid ASCII/UTF-8
        PooledArray<byte> buffer = ArrayPool.Rent<byte>(base64.Length);
        Span<byte> bufferSpan = buffer.AsSpan();
        int bytesWritten = Encoding.UTF8.GetBytes(base64, bufferSpan);
        // but we still need to check that that's the case
        if (bytesWritten != buffer.Length)
        {
            Log.WriteWarning("[SECURITY] Failed to decode base64 string. The provided string contains non-ASCII characters.");
            status = ClaimRepositoryStatus.Invalid;
            goto FAIL_INVALID_FORMAT;
        }
        // verify that the JWT has the correct format
        int headerSize = bufferSpan.IndexOf((byte)'.');
        int payloadSize = bufferSpan[(headerSize + 1)..].IndexOf((byte)'.');
        if (headerSize < 0 || payloadSize < 0)
        {
            Log.WriteWarning("[SECURITY] Failed to decode base64 string. The provided string is not a valid JWT.");
            status = ClaimRepositoryStatus.Invalid;
            goto FAIL_INVALID_FORMAT;
        }
        // copy and decode the header
        if (headerSize > 1024)
        {
            Log.WriteWarning("[SECURITY] Encountered unusually long JWT header. Rejecting invalid claim scope data.");
            status = ClaimRepositoryStatus.Invalid;
            goto FAIL_INVALID_FORMAT;
        }
        // ensure to copy the header to a separate buffer to avoid in-place decoding
        Span<byte> encodedHeader = stackalloc byte[headerSize];
        bufferSpan[..headerSize].CopyTo(encodedHeader);
        if (!Base64UrlEncoder.TryBase64UrlDecodeInPlace(encodedHeader, out Span<byte> header))
        {
            Log.WriteWarning("[SECURITY] Failed to decode JWT header. The provided string is not a valid base64 string.");
            status = ClaimRepositoryStatus.Invalid;
            goto FAIL_INVALID_FORMAT;
        }
        JwtHeader? jwtHeader = JsonSerializer.Deserialize<JwtHeader>(header);
        if (jwtHeader is null)
        {
            Log.WriteWarning("[SECURITY] Invalid JWT header format. Rejecting invalid claim scope data.");
            status = ClaimRepositoryStatus.Invalid;
            goto FAIL_INVALID_FORMAT;
        }
        if (jwtHeader.Algorithm != signingFunction.Name)
        {
            Log.WriteWarning("[SECURITY] Unsupported JWT algorithm. Rejecting invalid claim scope data.");
            status = ClaimRepositoryStatus.Invalid;
            goto FAIL_INVALID_FORMAT;
        }
        // okay, now validate the HMAC
        // first we need to url-decode the expected HMAC
        Span<byte> expectedSignatureBase64 = bufferSpan[(headerSize + payloadSize + 2)..];
        if (!Base64UrlEncoder.TryBase64UrlDecodeInPlace(expectedSignatureBase64, out Span<byte> expectedSignature))
        {
            Log.WriteWarning("[SECURITY] Failed to decode JWT HMAC. The provided string is not a valid base64 string.");
            status = ClaimRepositoryStatus.Invalid;
            goto FAIL_INVALID_FORMAT;
        }
        if (expectedSignature.Length != signingFunction.SignatureLengthInBytes)
        {
            Log.WriteWarning("[SECURITY] Invalid JWT HMAC length. Rejecting invalid claim scope data.");
            status = ClaimRepositoryStatus.Invalid;
            goto FAIL_INVALID_FORMAT;
        }
        // now extract the payload to retrieve the correct session key
        // work on a copy of the buffer since we need the original buffer for the HMAC computation
        PooledArray<byte> payloadBuffer = ArrayPool.Rent<byte>(payloadSize);
        Span<byte> payloadBufferSpan = payloadBuffer.AsSpan();
        bufferSpan[(headerSize + 1)..(headerSize + payloadSize + 1)].CopyTo(payloadBufferSpan);
        if (!Base64UrlEncoder.TryBase64UrlDecodeInPlace(payloadBufferSpan, out Span<byte> payload))
        {
            Log.WriteWarning("[SECURITY] Failed to decode JWT payload. The provided string is not a valid base64 string.");
            status = ClaimRepositoryStatus.Invalid;
            goto FAIL_INVALID_PAYLOAD;
        }
        // attempt to json deserialize the payload
        data = JsonSerializer.Deserialize<ClaimRepositoryData<TIdentityClaim, TDecryptionKeys>>(payload);
        if (data?.IdentityClaim is null)
        {
            Log.WriteWarning("[SECURITY] Failed to deserialize claim scope data.");
            status = ClaimRepositoryStatus.Invalid;
            goto FAIL_INVALID_PAYLOAD;
        }
        // the IdentityClaim.RawValue should never be null, but it might be if someone screws up their JSON serialization or the user-supplied data is invalid (HMAC mismatch)
        if (data.IdentityClaim.RawValue is null)
        {
            Log.WriteError("[SECURITY] IdentityClaim.RawValue is null. Are you sure JSON serialization is working correctly? Rejecting invalid claim scope data.");
            status = ClaimRepositoryStatus.Invalid;
            goto FAIL_INVALID_PAYLOAD;
        }
        // attempt to retrieve the in-memory session key for the IdentityClaim
        if (!sessions.TryGetSession(data.IdentityClaim.RawValue, out SessionKey<TDecryptionKeys>? sessionKey))
        {
            Log.WriteWarning($"[SECURITY] Invalid or expired session key: {data.IdentityClaim.RawValue}.");
            // although the data could not be validated, we still return it to the caller (data is not null)
            // they might be able to re-authenticate the user and recover the session through whatever context they have from the claim data
            status = ClaimRepositoryStatus.Expired;
            goto FAIL_INVALID_PAYLOAD;
        }
        // the computed HMAC will fit on the stack
        Span<byte> signature = stackalloc byte[signingFunction.SignatureLengthInBytes];
        // compute the HMAC using the server secret and the session key
        ReadOnlySpan<byte> headerAndPayload = bufferSpan[..(headerSize + payloadSize + 1)];
        bytesWritten = signingFunction.Sign(sessionKey, headerAndPayload, signature);
        Debug.Assert(bytesWritten == signingFunction.SignatureLengthInBytes);
        // compare the computed HMAC with the one from the base64 string
        if (!signature.SequenceEqual(expectedSignature))
        {
            Log.WriteError($"[SECURITY] Session key HMAC mismatch. This may indicate an attempt to tamper with the session data for IdentityClaim {data.IdentityClaim.RawValue}.");
            status = ClaimRepositoryStatus.Invalid;
            goto FAIL_VALIDATION;
        }
        // validate the expiration date and the claims
        if (data.ExpirationDate < DateTime.UtcNow)
        {
            Log.WriteWarning($"[SECURITY] Session key for IdentityClaim {data.IdentityClaim.RawValue} has expired.");
            sessions.TryRevokeSession(data.IdentityClaim.RawValue);
            status = ClaimRepositoryStatus.Expired;
            goto FAIL_VALIDATION;
        }
        // NULL-values in the claims are not allowed and are an indication of previously failed JSON serialization (tampering would be detected by the HMAC check)
        if (data.Claims.Any(c => c.RawValue is null))
        {
            Log.WriteWarning($"[SECURITY] One or more claims in the session key for IdentityClaim {data.IdentityClaim.RawValue} are null. Is your JSON serialization working correctly? Rejecting invalid claim scope data.");
            status = ClaimRepositoryStatus.Invalid;
            goto FAIL_VALIDATION;
        }
        // yay :)
        Log.WriteDiagnostic($"[SECURITY] Audit success: Session key for IdentityClaim {data.IdentityClaim.RawValue} has been validated.");
        // don't forget to include the decryption keys in our response for user-code to apply custom decryption of protected claims
        data.DecryptionKeys = sessionKey.DecryptionKeys;
        status = ClaimRepositoryStatus.Valid;
        ArrayPool.Return(payloadBuffer);
        ArrayPool.Return(buffer);
        return true;
    FAIL_VALIDATION:
    FAIL_INVALID_PAYLOAD:
        ArrayPool.Return(payloadBuffer);
    FAIL_INVALID_FORMAT:
        ArrayPool.Return(buffer);
        return false;
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
