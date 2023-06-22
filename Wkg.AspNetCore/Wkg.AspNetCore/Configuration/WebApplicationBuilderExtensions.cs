using Microsoft.AspNetCore.Builder;

namespace Wkg.AspNetCore.Configuration;

public static class WebApplicationBuilderExtensions
{
    public static WebApplication BuildUsing<TStartup>(this WebApplicationBuilder builder) where TStartup : IStartup
    {
        TStartup.ConfigureServices(builder.Services);
        WebApplication app = builder.Build();
        TStartup.Configure(app);
        return app;
    }
}