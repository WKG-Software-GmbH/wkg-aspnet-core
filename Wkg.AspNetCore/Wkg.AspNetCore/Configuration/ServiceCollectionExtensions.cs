using Microsoft.Extensions.DependencyInjection;
using System.Runtime.CompilerServices;

namespace Wkg.AspNetCore.Configuration;

public static class ServiceCollectionExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IServiceCollection ConfigureUsing<TStartupScript>(this IServiceCollection servcies) where TStartupScript : IStartupScript
    {
        TStartupScript.ConfigureServices(servcies);
        return servcies;
    }
}