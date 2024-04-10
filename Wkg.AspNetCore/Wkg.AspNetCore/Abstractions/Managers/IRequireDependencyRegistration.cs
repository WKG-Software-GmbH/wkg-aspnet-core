using Microsoft.Extensions.DependencyInjection;

namespace Wkg.AspNetCore.Abstractions.Managers;

/// <summary>
/// Represents an object that requries the registration of its dependencies.
/// </summary>
public interface IRequireDependencyRegistration
{
    /// <summary>
    /// Registers the required services for the object.
    /// </summary>
    /// <param name="services">The service collection to register the services with.</param>
    protected internal static abstract void RegisterRequiredServices(IServiceCollection services);
}
