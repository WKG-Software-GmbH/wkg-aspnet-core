namespace Wkg.AspNetCore.Authentication.Claims;

partial class Claim
{
    public static Claim Create(string subject, string value) => new(subject, value);

    public static Claim<TValue> Create<TValue>(string subject, TValue value) => new(subject, value);
}