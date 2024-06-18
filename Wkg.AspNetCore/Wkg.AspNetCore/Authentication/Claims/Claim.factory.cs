namespace Wkg.AspNetCore.Authentication.Claims;

partial class Claim
{
    /// <summary>
    /// Creates a new, basic claim with the specified subject and value.
    /// </summary>
    /// <param name="subject">The subject of the claim.</param>
    /// <param name="value">The value of the claim.</param>
    public static Claim Create(string subject, string value) => new(subject, value);

    /// <summary>
    /// Creates a new, typed claim with the specified subject and value.
    /// </summary>
    /// <remarks>
    /// Instances of <typeparamref name="TValue"/> must be JSON-serializable.
    /// </remarks>
    /// <typeparam name="TValue">The type of the value.</typeparam>
    /// <param name="subject">The subject of the claim.</param>
    /// <param name="value">The value of the claim.</param>
    public static Claim<TValue> Create<TValue>(string subject, TValue value) => new(subject, value);
}