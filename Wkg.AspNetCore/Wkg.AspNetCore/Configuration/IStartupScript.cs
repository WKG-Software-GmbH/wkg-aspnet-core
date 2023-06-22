using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Wkg.AspNetCore.Configuration;

public interface IStartupScript
{
    static abstract void Configure(WebApplication app);

    static abstract void ConfigureServices(IServiceCollection services);
}