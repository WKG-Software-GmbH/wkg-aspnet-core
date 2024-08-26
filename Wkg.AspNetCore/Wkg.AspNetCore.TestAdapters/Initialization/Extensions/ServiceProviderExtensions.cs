using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using Wkg.Unmanaged;

namespace Wkg.AspNetCore.TestAdapters.Initialization.Extensions;

/// <summary>
/// Extension methods for <see cref="IServiceProvider"/>.
/// </summary>
public static class ServiceProviderExtensions
{
    internal static T Activate<T>(this IServiceProvider serviceProvider) where T : class
    {
        Type type = typeof(T);
        ConstructorInfo[] constructors = type.GetConstructors();
        if (constructors is not [ConstructorInfo constructorInfo])
        {
            throw new InvalidOperationException($"Type {type} must have exactly one constructor.");
        }
        object?[] dependencies = constructorInfo
            .GetParameters()
            .Select(param => param.IsOptional 
                ? serviceProvider.GetService(param.ParameterType)
                : serviceProvider.GetRequiredService(param.ParameterType))
            .ToArray();
        return constructorInfo.Invoke(dependencies).ReinterpretAs<T>();
    }

    /// <summary>
    /// Initializes the database using the specified database loader.
    /// </summary>
    /// <typeparam name="TTestDatabaseLoader">The type of the database loader to be used to initialize the database with test data.</typeparam>
    /// <param name="serviceProvider">The service provider to be used to resolve the database loader and the database context.</param>
    public static void InitializeTestDatabase<TTestDatabaseLoader>(this IServiceProvider serviceProvider)
        where TTestDatabaseLoader : class, ITestDatabaseLoader => TTestDatabaseLoader.InitializeDatabase(serviceProvider);
}