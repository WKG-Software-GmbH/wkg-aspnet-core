using Microsoft.Extensions.DependencyInjection;
using System.Runtime.CompilerServices;

namespace Wkg.AspNetCore.Configuration;

/// <summary>
/// Extension methods for <see cref="IServiceCollection"/>.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Configures the <see cref="IServiceCollection"/> using the specified <typeparamref name="TStartupScript"/>.
    /// </summary>
    /// <typeparam name="TStartupScript">The type of the startup script.</typeparam>
    /// <param name="servcies">The <see cref="IServiceCollection"/>.</param>
    /// <returns>The <see cref="IServiceCollection"/> for fluent configuration.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IServiceCollection ConfigureUsing<TStartupScript>(this IServiceCollection servcies) where TStartupScript : IStartupScript
    {
        TStartupScript.ConfigureServices(servcies);
        return servcies;
    }
}