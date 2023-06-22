using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Wkg.AspNetCore.Configuration;

public static class ServiceCollectionExtensions
{
    public static void ConfigureUsing<TStartupScript>(this IServiceCollection servcies) where TStartupScript : IStartupScript => 
        TStartupScript.ConfigureServices(servcies);
}