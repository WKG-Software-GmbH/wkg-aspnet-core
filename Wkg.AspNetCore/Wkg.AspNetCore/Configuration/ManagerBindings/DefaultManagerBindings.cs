using Microsoft.Extensions.DependencyInjection;
using Wkg.AspNetCore.Abstractions;

namespace Wkg.AspNetCore.Configuration.ManagerBindings;

internal class DefaultManagerBindings(ManagerBindingOptions _options, IServiceProvider _scopedServiceProvider) : IManagerBindings
{
    TManager IManagerBindings.ActivateManager<TManager>(IMvcContext<TManager> context)
    {
        if (_options.Map.TryGetValue(typeof(TManager), out ManagerFactory? factory))
        {
            // the manager exists. We know we are running in a scoped context, so we can safely create the manager
            object managerObject = factory.Invoke(_scopedServiceProvider);
            // reinterpret_cast because we know the type is correct by convention (the map is built from the same types)
            TManager manager = managerObject.ReinterpretAs<TManager>();
            manager.Context = context;
            return manager;
        }
        throw new InvalidOperationException($"Manager of type {typeof(TManager).Name} is not recognized.");
    }
}