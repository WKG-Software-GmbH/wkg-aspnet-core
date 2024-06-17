using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization;
using Wkg.AspNetCore.Authentication.Claims;

namespace Wkg.AspNetCore.Authentication;

partial class Claim
{
    public static Claim Create(string subject, string value) => new(subject, value);

    public static Claim<TValue> Create<TValue>(string subject, TValue value) => new(subject, value);
}