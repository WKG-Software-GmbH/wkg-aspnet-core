using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Wkg.AspNetCore.TestAdapters.Extensions;

internal static class ServiceProviderExtensions
{
    public static T ActivateController<T>(this ServiceProvider serviceProvider) where T : ControllerBase
    {
        Type type = typeof(T);
        ConstructorInfo constructorInfo = type
            .GetConstructors()
            .First();
        object?[] dependencies = constructorInfo
            .GetParameters()
            .Select(param => serviceProvider
                .GetService(param.ParameterType))
            .ToArray();
        return (T)constructorInfo.Invoke(dependencies);
    }
}