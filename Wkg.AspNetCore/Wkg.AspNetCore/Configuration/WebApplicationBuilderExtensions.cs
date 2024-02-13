using Microsoft.AspNetCore.Builder;
using System.Runtime.CompilerServices;

namespace Wkg.AspNetCore.Configuration;

/// <summary>
/// Extension methods for <see cref="WebApplicationBuilder"/>.
/// </summary>
public static class WebApplicationBuilderExtensions
{
    /// <summary>
    /// Configures the provided <see cref="WebApplicationBuilder"/> and the resulting <see cref="WebApplication"/> using the specified <typeparamref name="TStartupScript"/>.
    /// </summary>
    /// <typeparam name="TStartupScript">The type of the startup script.</typeparam>
    /// <param name="builder">The builder.</param>
    /// <returns>The configured <see cref="WebApplication"/>.</returns>
    public static WebApplication BuildUsing<TStartupScript>(this WebApplicationBuilder builder) where TStartupScript : IStartupScript
    {
        builder.ConfigureServicesUsing<TStartupScript>();
        WebApplication app = builder.Build();
        TStartupScript.Configure(app);
        return app;
    }

    /// <summary>
    /// Configures the <see cref="WebApplicationBuilder.Services"/> of the provided <paramref name="builder"/> using the specified <typeparamref name="TStartupScript"/>.
    /// </summary>
    /// <typeparam name="TStartupScript">The type of the startup script.</typeparam>
    /// <param name="builder">The builder.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ConfigureServicesUsing<TStartupScript>(this WebApplicationBuilder builder) where TStartupScript : IStartupScript => 
        TStartupScript.ConfigureServices(builder.Services, builder.Configuration);
}