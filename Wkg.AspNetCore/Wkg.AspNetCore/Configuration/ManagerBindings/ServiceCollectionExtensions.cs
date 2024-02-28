using Microsoft.Extensions.DependencyInjection;
using System.Collections.Frozen;
using System.Linq.Expressions;
using System.Reflection;
using Wkg.AspNetCore.Abstractions;
using Wkg.Reflection;
using Wkg.Reflection.Extensions;

namespace Wkg.AspNetCore.Configuration.ManagerBindings;

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
        // collect all manager types used by concrete MvcContext types (controllers, razor pages, etc.)
        IEnumerable<Type> managers = AppDomain.CurrentDomain
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
                // get the manager type (IMvcContext<TManager>)
                // assume that every MvcContext implements the IMvcContext<TManager> interface exactly once
                // otherwise: wtf are you doing?
                .Select(t => t.GetGenericTypeArgumentOfSingleInterface(typeof(IMvcContext<>))!))
            // important: multiple MvcContext types may use the backing manager implementation
            // for example a RazorPage and a Controller may both use the same implementation
            .Distinct();

        // create a frozen dictionary of manager types and their DI-aware factories
        FrozenDictionary<Type, ManagerFactory> factories = managers
            .ToDictionary(
                binding => binding,
                CreateFactory)
            .ToFrozenDictionary();
        ManagerBindingOptions bindingOptions = new(factories);
        services.AddSingleton(bindingOptions);
        services.AddSingleton<IManagerBindings, DefaultManagerBindings>();
        return services;
    }

    // for use in expression trees
    private static readonly MethodInfo _serviceProviderGetRequiredService = typeof(ServiceProviderServiceExtensions).GetMethod(
            nameof(ServiceProviderServiceExtensions.GetRequiredService),
            BindingFlags.Public | BindingFlags.Static,
            TypeArray.Of<IServiceProvider, Type>())
        ?? throw new InvalidOperationException("Method GetRequiredService not found.");

    // Create a DI-aware factory for the manager type.
    private static ManagerFactory CreateFactory(Type managerType)
    {
        if (managerType.IsAbstract)
        {
            throw new InvalidOperationException($"Manager {managerType.Name} must be concrete.");
        }
        ConstructorInfo[] constructors = managerType.GetConstructors();
        if (constructors.Length != 1)
        {
            throw new InvalidOperationException($"Manager {managerType.Name} must have exactly one publicly accessible constructor. Currently, there are {constructors.Length}!");
        }
        // build a lambda expression that takes an IServiceProvider, retrieves the required services, and invokes the constructor
        ConstructorInfo constructor = constructors[0];
        ParameterInfo[] parameters = constructor.GetParameters();
        ParameterExpression serviceProvider = Expression.Parameter(typeof(IServiceProvider), "serviceProvider");
        // new TManager(Unsafe.As<typeof(param1)>(serviceProvider.GetRequiredService(typeof(param1))), ...);
        Expression[] arguments = parameters
            .Select(param =>
                Expression.Call(UnsafeReflection.As(param.ParameterType),
                    Expression.Call(_serviceProviderGetRequiredService,
                        serviceProvider,
                        Expression.Constant(param.ParameterType))))
            .ToArray();
        NewExpression newExpression = Expression.New(constructor, arguments);
        Expression<ManagerFactory> lambda = Expression.Lambda<ManagerFactory>(newExpression, serviceProvider);
        return lambda.Compile();
    }
}
