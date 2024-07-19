using Wkg.AspNetCore.TestAdapters.Initialization;
using Wkg.AspNetCore.TestAdapters.Initialization.Extensions;

namespace Wkg.AspNetCore.TestAdapters;

/// <summary>
/// Represents the base class for tests of ASP.NET Core components that require dependency injection services.
/// </summary>
/// <typeparam name="TComponent">The type of the component to be tested. Must be activatable using DI.</typeparam>
/// <typeparam name="TInitializer">The <see cref="IDITestInitializer"/> implementation to be used for test initialization.</typeparam>
public abstract class ComponentTest<TComponent, TInitializer> : DIAwareTestBase<TInitializer>
    where TComponent : class
    where TInitializer : IDITestInitializer
{
    /// <summary>
    /// Executes the specified unit test against the component in a dedicated DI scope.
    /// </summary>
    /// <param name="unitTestAction">The unit test action to be executed.</param>
    protected Task UsingComponentAsync(Action<TComponent> unitTestAction) => UsingServiceProviderAsync(serviceProvider =>
    {
        TComponent component = serviceProvider.Activate<TComponent>();
        unitTestAction.Invoke(component);
    });

    /// <summary>
    /// Executes the specified unit test asynchronously against the component in a dedicated DI scope.
    /// </summary>
    /// <param name="unitTestTask">The unit test task to be executed asynchronously.</param>
    protected Task UsingComponentAsync(Func<TComponent, Task> unitTestTask) => UsingServiceProviderAsync(async serviceProvider =>
    {
        TComponent component = serviceProvider.Activate<TComponent>();
        await unitTestTask.Invoke(component);
    });
}
