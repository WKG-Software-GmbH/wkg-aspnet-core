using Microsoft.AspNetCore.Builder;
using System.Runtime.CompilerServices;

namespace Wkg.AspNetCore.Configuration;

public static class WebApplicationBuilderExtensions
{
    public static WebApplication BuildUsing<TStartupScript>(this WebApplicationBuilder builder) where TStartupScript : IStartupScript
    {
        builder.ConfigureServicesUsing<TStartupScript>();
        WebApplication app = builder.Build();
        TStartupScript.Configure(app);
        return app;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ConfigureServicesUsing<TStartupScript>(this WebApplicationBuilder builder) where TStartupScript : IStartupScript => 
        TStartupScript.ConfigureServices(builder.Services);
}