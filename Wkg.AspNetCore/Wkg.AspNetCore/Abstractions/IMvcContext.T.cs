using Wkg.AspNetCore.Abstractions.Managers;

namespace Wkg.AspNetCore.Abstractions;

internal interface IMvcContext<TManager> : IMvcContext where TManager : ManagerBase;