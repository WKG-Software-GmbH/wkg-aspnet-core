using System.Text.Json.Serialization;

namespace Wkg.AspNetCore.Authentication.Jwt;

internal record JwtHeader([property: JsonPropertyName("alg")] string Algorithm, [property: JsonPropertyName("typ")] string Type = "JWT");