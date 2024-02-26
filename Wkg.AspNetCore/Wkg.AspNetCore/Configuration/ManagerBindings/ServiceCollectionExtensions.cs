using Microsoft.Extensions.DependencyInjection;
using System.Collections.Frozen;
using System.Linq.Expressions;
using System.Reflection;
using Wkg.AspNetCore.Abstractions;
using Wkg.Reflection;
using Wkg.Reflection.Extensions;

namespace Wkg.AspNetCore.Configuration.ManagerBindings;

using ManagerBinding = (Type ControllerType, Type ManagerType);

/// <summary>
/// Extension methods for <see cref="IServiceCollection"/>.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds support for manager bindings.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/>.</param>
    /// <returns>The <see cref="IServiceCollection"/> for fluent configuration.</returns>
    public static IServiceCollection AddManagers(this IServiceCollection services)
    {
        IEnumerable<ManagerBinding> bindings = AppDomain.CurrentDomain
            // get all assemblies
            .GetAssemblies()
            // filter out system and microsoft assemblies
            .Where(asm => asm.FullName is string name && !name.StartsWith(nameof(System)) && !name.StartsWith(nameof(Microsoft)))
            // get all exported (public) types
            .SelectMany(assembly => assembly.GetExportedTypes()
                // only keep concrete MvcContext types
                .Where(t =>
                    !t.IsAbstract
                    && t.IsAssignableTo(typeof(IMvcContext))
                    && t.ImplementsGenericInterface(typeof(IMvcContext<>)))
                .Select(t =>
                (
                    ControllerType: t,
                    // get the corresponding Manager type
                    ManagerType: t.GetGenericTypeArgumentOfSingleInterface(typeof(IMvcContext<>))!
                )));
        FrozenDictionary<Type, ManagerFactory> factories = bindings
            .ToDictionary(
                binding => binding.ControllerType,
                CreateFactory)
            .ToFrozenDictionary();
        ManagerBindingOptions bindingOptions = new(factories);
        services.AddSingleton(bindingOptions);
        services.AddSingleton<IManagerBindings, DefaultManagerBindings>();
        return services;
    }

    private static readonly MethodInfo _serviceProviderGetRequiredService = typeof(ServiceProviderServiceExtensions).GetMethod(
            nameof(ServiceProviderServiceExtensions.GetRequiredService),
            BindingFlags.Public | BindingFlags.Static,
            TypeArray.Of<IServiceProvider, Type>())
        ?? throw new InvalidOperationException("Method GetRequiredService not found.");

    private static ManagerFactory CreateFactory(ManagerBinding managerBinding)
    {
        if (managerBinding.ManagerType.IsAbstract)
        {
            throw new InvalidOperationException($"Manager {managerBinding.ManagerType.Name} must be concrete.");
        }
        ConstructorInfo[] constructors = managerBinding.ManagerType.GetConstructors(BindingFlags.Public);
        if (constructors.Length != 1)
        {
            throw new InvalidOperationException($"Manager {managerBinding.ManagerType.Name} must have exactly one publicly accessible constructor.");
        }
        ConstructorInfo constructor = constructors[0];
        ParameterInfo[] parameters = constructor.GetParameters();
        ParameterExpression serviceProvider = Expression.Parameter(typeof(IServiceProvider), "serviceProvider");
        // new TManager(serviceProvider.GetRequiredService(param1), ...);
        Expression[] arguments = parameters
            .Select(param =>
                Expression.Call(_serviceProviderGetRequiredService,
                    serviceProvider,
                    Expression.Constant(param.ParameterType)))
            .ToArray();
        NewExpression newExpression = Expression.New(constructor, arguments);
        Expression<ManagerFactory> lambda = Expression.Lambda<ManagerFactory>(newExpression, serviceProvider);
        return lambda.Compile();
    }
}
