using Wkg.AspNetCore.Abstractions;

namespace Wkg.AspNetCore.Configuration.ManagerBindings;

internal class DefaultManagerBindings(ManagerBindingOptions _options, IServiceProvider _serviceProvider) : IManagerBindings
{
    TManager IManagerBindings.ActivateManager<TManager>(IMvcContext context)
    {
        if (_options.Map.TryGetValue(typeof(TManager), out ManagerFactory? factory))
        {
            // reinterpret_cast because we know the type is correct by convention (the map is built from the same types)
            TManager manager = factory.Invoke(_serviceProvider).ReinterpretAs<TManager>();
            manager.Context = context;
            return manager;
        }
        throw new InvalidOperationException($"Manager of type {typeof(TManager).Name} is not recognized.");
    }
}