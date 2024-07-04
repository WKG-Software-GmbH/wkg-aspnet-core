using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using Wkg.Unmanaged;

namespace Wkg.AspNetCore.TestAdapters.Extensions;

internal static class ServiceProviderExtensions
{
    public static T Activate<T>(this IServiceProvider serviceProvider) where T : class
    {
        Type type = typeof(T);
        ConstructorInfo[] constructors = type.GetConstructors();
        if (constructors.Length != 1)
        {
            throw new InvalidOperationException($"Type {type} must have exactly one constructor.");
        }
        ConstructorInfo constructorInfo = constructors[0];
        object[] dependencies = constructorInfo
            .GetParameters()
            .Select(param => serviceProvider
                .GetRequiredService(param.ParameterType))
            .ToArray();
        return constructorInfo.Invoke(dependencies).ReinterpretAs<T>();
    }
}