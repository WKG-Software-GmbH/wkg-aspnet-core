using Microsoft.Extensions.DependencyInjection;
using Wkg.AspNetCore.Abstractions;

namespace Wkg.AspNetCore.Configuration.ManagerBindings;

internal class DefaultManagerBindings(ManagerBindingOptions _options, IServiceProvider _serviceProvider) : IManagerBindings
{
    TManager IManagerBindings.ActivateManager<TManager>(IMvcContext<TManager> context)
    {
        if (_options.Map.TryGetValue(typeof(TManager), out ManagerFactory? factory))
        {
            // the manager exists. Create a service scope and activate the manager
            // this scope must be disposed by the context
            IServiceScope scope = _serviceProvider.CreateScope();
            context.ServiceScope = scope;
            // ensure to use the scoped service provider
            object managerObject = factory.Invoke(scope.ServiceProvider);
            // reinterpret_cast because we know the type is correct by convention (the map is built from the same types)
            TManager manager = managerObject.ReinterpretAs<TManager>();
            manager.Context = context;
            return manager;
        }
        throw new InvalidOperationException($"Manager of type {typeof(TManager).Name} is not recognized.");
    }
}