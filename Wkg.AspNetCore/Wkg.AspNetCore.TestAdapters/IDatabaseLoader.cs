using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Wkg.AspNetCore.TestAdapters;

/// <summary>
/// Represents setup code that is executed before the first test of the first test class is run.
/// </summary>
public interface IDatabaseLoader
{
    /// <summary>
    /// Initializes the database with the data that is required for the tests.
    /// </summary>
    /// <param name="serviceProvider">The <see cref="ServiceProvider"/> that can be used to retrieve the configured <see cref="DbContext"/> instances.</param>
    static abstract void InitializeDatabase(ServiceProvider serviceProvider);
}
