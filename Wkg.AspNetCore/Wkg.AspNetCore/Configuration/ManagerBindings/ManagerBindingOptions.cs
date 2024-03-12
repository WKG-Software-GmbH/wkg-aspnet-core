using System.Collections.Frozen;

namespace Wkg.AspNetCore.Configuration.ManagerBindings;

internal record ManagerBindingOptions(FrozenDictionary<Type, ManagerFactory> Map);
