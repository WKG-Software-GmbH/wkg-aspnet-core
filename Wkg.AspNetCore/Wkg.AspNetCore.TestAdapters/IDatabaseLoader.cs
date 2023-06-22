using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wkg.AspNetCore.TestAdapters;

public interface IDatabaseLoader
{
    static abstract void InitializeDatabase(ServiceProvider serviceProvider);
}
