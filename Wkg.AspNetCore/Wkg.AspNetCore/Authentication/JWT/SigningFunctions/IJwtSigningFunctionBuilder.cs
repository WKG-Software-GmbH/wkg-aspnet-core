using Microsoft.Extensions.DependencyInjection;

namespace Wkg.AspNetCore.Authentication.Jwt.SigningFunctions;

/// <summary>
/// Represents a builder for a JWT signing function.
/// </summary>
public interface IJwtSigningFunctionBuilder
{
    internal void Build(IServiceCollection services);
}

/// <summary>
/// Represents a builder for a JWT signing function.
/// </summary>
public interface IJwtSigningFunctionBuilder<TSelf> : IJwtSigningFunctionBuilder where TSelf : IJwtSigningFunctionBuilder<TSelf>
{
    internal static abstract TSelf Create();

    /// <summary>
    /// Sets the server secret to use for signing the JWT token.
    /// </summary>
    /// <param name="secret">The server secret to use for signing the JWT token. Should be a random string, as this protects the integrity of all JWT tokens.</param>
    /// <returns>The current instance of the <typeparamref name="TSelf"/>.</returns>
    TSelf UsingSecret(string secret);
}