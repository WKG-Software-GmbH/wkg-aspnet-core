using Wkg.AspNetCore.Abstractions;
using Wkg.AspNetCore.Abstractions.Managers;

namespace Wkg.AspNetCore.Configuration.ManagerBindings;

internal class DefaultManagerBindings(ManagerBindingOptions _options, IServiceProvider _scopedServiceProvider) : IManagerBindings
{
    private Dictionary<Type, ManagerBase>? _scopedManagerCache;

    public TManager ActivateManagerUnsafe<TManager>(IMvcContext context) where TManager : ManagerBase
    {
        _scopedManagerCache ??= [];
        if (_scopedManagerCache.TryGetValue(typeof(TManager), out ManagerBase? cachedManager))
        {
            return cachedManager.ReinterpretAs<TManager>();
        }
        TManager manager = ActivateManagerCore<TManager>(context);
        _scopedManagerCache.Add(typeof(TManager), manager);
        return manager;
    }

    TManager IManagerBindings.ActivateManager<TManager>(IMvcContext<TManager> context) => ActivateManagerCore<TManager>(context);

    private TManager ActivateManagerCore<TManager>(IMvcContext context) where TManager : ManagerBase
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