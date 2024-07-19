# `Wkg.AspNetCore` Documentation

`Wkg.AspNetCore` is a company-internal library providing reusable components for the development of ASP\.NET Core applications at WKG.

- [`Wkg.AspNetCore` Documentation](#wkgaspnetcore-documentation)
  - [`Wkg.AspNetCore`](#wkgaspnetcore)
    - [`Abstractions` Namespace](#abstractions-namespace)
      - [Introducing Manager Classes](#introducing-manager-classes)
      - [Communicating Results with `ManagerResult`](#communicating-results-with-managerresult)
        - [Converting Between Result Types](#converting-between-result-types)
      - [Consuming Multiple Manager Classes](#consuming-multiple-manager-classes)
    - [`ErrorHandling` Namespace](#errorhandling-namespace)
    - [`Transactions` Namespace](#transactions-namespace)
      - [Configuration](#configuration)
      - [Using Transactions](#using-transactions)
      - [Consuming `ITransactionService` Directly](#consuming-itransactionservice-directly)
      - [Creating Custom Transaction Scopes](#creating-custom-transaction-scopes)
    - [`Validation` Namespace](#validation-namespace)
  - [`Wkg.AspNetCore.TestAdapters`](#wkgaspnetcoretestadapters)
    - [Setup](#setup)
    - [Writing Tests](#writing-tests)
      - [Testing Simple Classes](#testing-simple-classes)
      - [Testing Components Using DI](#testing-components-using-di)
      - [Testing Database Access](#testing-database-access)
        - [Database Loaders](#database-loaders)
        - [Using the DbContext Directly in Tests](#using-the-dbcontext-directly-in-tests)

## `Wkg.AspNetCore`

### `Abstractions` Namespace

A core part of the `Wkg.AspNetCore` library is the `Abstractions` namespace, which provides new abstractions over existing ASP\.NET Core components. These abstractions are designed to extend and unify the existing ASP\.NET Core components, such as API controllers, Razor pages, and services.

#### Introducing Manager Classes

In an effort to decouple business logic from architectural concerns like REST API endpoints or web page rendering, `Wkg.AspNetCore` introduces the concept of **managers**. Managers are classes that encapsulate business logic and expose it through a set of methods. They are designed to be used by controllers, pages, or other services to perform business operations. This is achieved by moving core controller and page logic into manager classes, which are then injected into controllers or pages as dependencies, allowing them to be easily tested and reused.

The following example demonstrates how a manager class can be used to encapsulate and share business logic between a controller and a razor page:

```csharp
// Program.cs
// discover managers, create bindings between manager classes and their dependencies, and prepare DI for the manager classes
builder.Services.AddManagers();

// WeatherManager.cs
// manager classes consume services and encapsulate business logic. 
// They are more specialized than services and are designed to be used by controllers or pages.
public class WeatherManager(IWeatherService weatherService, IUserService userService, IConversionService converter) : ManagerBase
{
    public async Task<WeatherForecast> GetWeatherForecastAsync(string userId)
    {
        User user = await _userService.GetUserByIdAsync(userId);
        WeatherForecast weatherForecast = await _weatherService.GetWeatherForecastAsync(user.Location);
        foreach (WeatherForecastEntry entry in weatherForecast.Entries)
        {
            entry.Temperature = converter.Convert(entry.Temperature, user.PreferredTemperatureUnit);
        }
        return weatherForecast;
    }
}

// WeatherController.cs
// controllers use one or more manager classes to perform business operations and only contain additional 
// logic related to the REST API request/response cycle.
public class WeatherController(IManagerBindings managerBindings) : ManagerController<WeatherManager>(managerBindings)
{
    [HttpGet]
    public async Task<IActionResult> GetWeatherForecastAsync(string userId)
    {
        WeatherForecast weatherForecast = await Manager.GetWeatherForecastAsync(userId);
        return Ok(weatherForecast);
    }
}

// WeatherPage.cshtml.cs
// razor pages use one or more manager classes to perform business operations and only contain additional
// logic related to rendering the page.
public class WeatherPageModel(IManagerBindings managerBindings) : ManagerPageModel<WeatherManager>(managerBindings)
{
    public WeatherForecast WeatherForecast { get; private set; } = null!;

    public async Task<IActionResult> OnGetAsync(string userId)
    {
        WeatherForecast = await Manager.GetWeatherForecastAsync(userId);
        return Page();
    }
}
```

In this example, the `WeatherManager` class encapsulates the business logic for retrieving and processing weather forecasts using a variety of services. The `WeatherController` and `WeatherPageModel` classes then use the `WeatherManager` class to perform their respective tasks, such as returning a weather forecast as JSON or rendering a weather forecast on a web page.

In order to use manager classes, the `ManagerController` and `ManagerPageModel` base classes are provided by the `Wkg.AspNetCore` library. These base classes handle the injection of manager classes into controllers and pages, allowing them to be easily accessed and used.

By using manager classes in this way, business logic can be easily shared between different parts of an application, making it easier to maintain and test. Managers also help to enforce the separation of concerns between business logic and architectural concerns, leading to cleaner and more maintainable code.

#### Communicating Results with `ManagerResult`

The minimalistic example in [Introducing Manager Classes](#introducing-manager-classes) demonstrates how a manager class can be used to encapsulate business logic and share it between different parts of an application. However, in practice, business operations often require more complex error handling and result reporting.

To address this, `Wkg.AspNetCore` introduces the `ManagerResult` struct, which is used to represent the result of a business operation and to communicate response codes or error messages between manager classes and their consumers. The `ManagerResult` struct is designed to be used as a return type for all manager methods that may fail or produce an error.

The code snippet below demonstrates how the `ManagerResult` struct can be used to return a success or error result from a manager method:

```csharp
// WeatherManager.cs
public class WeatherManager(IWeatherService weatherService, IUserService userService, IConversionService converter) : ManagerBase
{
    public async Task<ManagerResult<WeatherForecast>> GetWeatherForecastAsync(string? userId)
    {
        if (userId is null)
        {
            return Unauthorized<WeatherForecast>("User ID is required.");
        }
        User? user = await _userService.GetUserByIdAsync(userId);
        if (user is null)
        {
            return Forbidden<WeatherForecast>("The user does not exist or is locked out.");
        }
        WeatherForecast? weatherForecast = await _weatherService.GetWeatherForecastAsync(user.Location);
        if (weatherForecast is null)
        {
            return NotFound<WeatherForecast>("No weather forecast available for the user's location.");
        }
        foreach (WeatherForecastEntry entry in weatherForecast.Entries)
        {
            entry.Temperature = converter.Convert(entry.Temperature, user.PreferredTemperatureUnit);
        }
        return Success(weatherForecast);
    }
}
```

In this modified example, the `GetWeatherForecastAsync` method returns a `ManagerResult<WeatherForecast>` instead of a `WeatherForecast`. This allows the method to communicate success or distinguish between different types of errors by returning a `ManagerResult` with an appropriate status code and message. The `ManagerResult` struct provides a set of static methods to create instances of `ManagerResult<TResult>` with different status codes and messages, such as `Success`, `Unauthorized`, `Forbidden`, and `NotFound`. These static methods are mirrored by the `ManagerBase` class, which provides convenience methods for creating `ManagerResult` instances.

By using the `ManagerResult` struct, manager classes can provide more detailed feedback to their consumers about the outcome of a business operation, allowing them to handle errors more effectively and provide better feedback to users. The `ManagerResult` struct also helps to enforce consistent error handling practices across an application, making it easier to maintain and test, as shown in the following example:

```csharp
// WeatherController.cs
public class WeatherController(IManagerBindings managerBindings) : ManagerController<WeatherManager>(managerBindings)
{
    [HttpGet]
    public async Task<IActionResult> GetWeatherForecastAsync(string? userId)
    {
        ManagerResult<WeatherForecast> result = await Manager.GetWeatherForecastAsync(userId);
        return Handle(result);
    }
}

// WeatherPage.cshtml.cs
public class WeatherPageModel(IManagerBindings managerBindings) : ManagerPageModel<WeatherManager>(managerBindings)
{
    public WeatherForecast WeatherForecast { get; private set; } = null!;

    public async Task<IActionResult> OnGetAsync(string? userId)
    {
        ManagerResult<WeatherForecast> result = await Manager.GetWeatherForecastAsync(userId);
        if (!result.TryGetResult(out WeatherForecast? weatherForecast))
        {
            return HandleNonSuccess(result);
        }
        WeatherForecast = weatherForecast;
        return Page();
    }
}
```

In this example, the `WeatherController` and `WeatherPageModel` classes use the `ManagerResult` struct to handle different types of errors returned by the `WeatherManager` class. The `Handle` and `HandleNonSuccess` methods are provided by the `ManagerController` and `ManagerPageModel` base classes, respectively, to handle the different types of `ManagerResult` instances returned by manager methods. Whereas the `Handle` method handles all types of `ManagerResult` instances, the `HandleNonSuccess` method only handles non-successful results, allowing the page or controller to handle successful results separately. Each method returns an appropriate `IActionResult` for the current context, allowing the page or controller to respond to errors or success conditions as needed. Internally, these methods mainly consist of switch statements that map the status codes of the `ManagerResult` instances to the appropriate `IActionResult` instances:

```csharp
// ManagerController::Handle
// a simplified version of the Handle method from the ManagerController base class
protected virtual IActionResult Handle<TResult>(ManagerResult<TResult> result) => result.StatusCode switch
{
    ManagerResultCode.Success => Ok(result.GetOrThrow()),
    ManagerResultCode.BadRequest => BadRequest(),
    ManagerResultCode.Unauthorized => Unauthorized(),
    ManagerResultCode.Forbidden => Forbid(),
    ManagerResultCode.InvalidModelState => BadRequest(ModelState),
    ManagerResultCode.NotFound => NotFound(),
    ManagerResultCode.InternalServerError => throw new Exception(result.ErrorMessage),
    _ => throw new ArgumentException($"{result.StatusCode} is not a valid result code.", nameof(result)),
};
```

##### Converting Between Result Types

In some cases, it may be necessary to convert between different types of `ManagerResult` instances, such as when a method returns a `ManagerResult<WeatherForecast>` but needs to return a `ManagerResult<WeatherForecastEntry[]>` instead. To facilitate this, the `ManagerResult` struct provides a set of conversion methods that allow you to convert between different types of `ManagerResult` instances while preserving the status code and message of the original result, such as shown in the following example:

```csharp
// WeatherManager.cs
public class WeatherManager(IWeatherService weatherService, IUserService userService, IConversionService converter) : ManagerBase
{
    ...
    public async Task<ManagerResult<WeatherForecastEntry[]>> GetWeatherForecastEntriesAsync(string? userId)
    {
        ManagerResult<WeatherForecast> result = await GetWeatherForecastAsync(userId);
        if (!result.TryGetResult(out WeatherForecast? weatherForecast))
        {
            // preserve the status code and message of the original result, but re-bind the generic type
            return result.FailureAs<WeatherForecastEntry[]>();
        }
        return Success(weatherForecast.Entries);
    }
}
```

Note that the `FailureAs<TResult>` method is only defined for `ManagerResult` instances that do not contain a result value. If the original result contains a result value, then `FailureAs<TResult>` will throw an exception, since the conversion between different result types may not be defined.

Similarly, result value information may be dropped intentionally by calling `WithoutValue()`, or by using the implicit conversion between `ManagerResult<TResult>` and `ManagerResult`:

```csharp
// UserManager.cs
public class UserManager(IUserService userService, IRoleService roleService) : ManagerBase
{
    public async Task<ManagerResult> AddUserToRoleAsync(string? userId, string? roleName)
    {
        ManagerResult<User> userResult = await userService.GetUserByIdAsync(userId);
        if (!userResult.TryGetResult(out User? user))
        {
            // drop the user value information and explicitly communicate to other developers that the user value is not needed
            return userResult.WithoutValue();
        }
        ManagerResult<Role> roleResult = await roleService.GetRoleByNameAsync(roleName);
        if (!roleResult.TryGetResult(out Role? role))
        {
            // implicitly drop the role value information
            return roleResult;
        }
        bool success = await userService.AddUserToRoleAsync(user, role);
        if (!success)
        {
            return InternalServerError("Failed to add user to role.");
        }
        return Success();
    }
}
```

#### Consuming Multiple Manager Classes

In some cases, a business operation may require the coordination of multiple manager classes to complete. This may be especially true for Razor pages, which may need to interact with multiple manager classes to render a page. To facilitate this, the `ManagerPageModel` and `ManagerController` base classes provide a factory method for retrieving manager instances other than the primary manager instance, allowing controllers and pages to consume multiple manager classes as needed.

Similarly, manager classes may interact with each other by retrieving instances of other manager classes from the `IManagerBindings` service, which is registered with the DI container by the `AddManagers` extension method. The `ManagerBindings` service provides a way to activate manager classes and resolve their dependencies, allowing manager classes to interact with each other without creating circular dependencies.

The following example demonstrates how a page model can consume multiple manager classes to render a page:

```csharp
// DashboardPage.cshtml.cs
public class DashboardPageModel(IManagerBindings managerBindings) : ManagerPageModel<WeatherManager>(managerBindings)
{
    public WeatherForecast WeatherForecast { get; private set; } = null!;
    public UVIndex UVIndex { get; private set; } = null!;
    public AirQualityIndex AirQualityIndex { get; private set; } = null!;

    public async Task<IActionResult> OnGetAsync(string? userId)
    {
        // interact with the primary manager instance
        ManagerResult<WeatherForecast> weatherResult = await Manager.GetWeatherForecastAsync(userId);
        if (!weatherResult.TryGetResult(out WeatherForecast? weatherForecast))
        {
            return HandleNonSuccess(weatherResult);
        }
        WeatherForecast = weatherForecast;
        // interact with other manager instances
        UVIndexManager uvManager = GetManager<UVIndexManager>();
        ManagerResult<UVIndex> uvResult = await uvManager.GetUVIndexAsync(userId);
        if (!uvResult.TryGetResult(out UVIndex? uvIndex))
        {
            return HandleNonSuccess(uvResult);
        }
        UVIndex = uvIndex;

        AirQualityIndexManager airQualityManager = GetManager<AirQualityIndexManager>();
        ManagerResult<AirQualityIndex> airQualityResult = await airQualityManager.GetAirQualityIndexAsync(userId);
        if (!airQualityResult.TryGetResult(out AirQualityIndex? airQualityIndex))
        {
            return HandleNonSuccess(airQualityResult);
        }
        AirQualityIndex = airQualityIndex;

        return Page();
    }
}
```

In this example, the `DashboardPageModel` class consumes multiple manager classes in addition to the primary `WeatherManager` instance to render a dashboard page. The `GetManager<TManager>` method is provided by the `ManagerPageModel` base class to retrieve instances of other manager classes from the `IManagerBindings` service, allowing the page model to interact with multiple manager classes as needed.

> :information_source: **Note:** Subsequent calls to `GetManager<TManager>` will return the same instance of the manager class, ensuring that the same manager instance is used consistently throughout the page model's lifecycle. All manager instances are resolved from the DI container and are subject to the same lifetime management as other scoped services.

### `ErrorHandling` Namespace

The `ErrorHandling` namespace provides the `IErrorSentry` interface which can be used to log exceptions and errors in a consistent manner. The `ErrorSentry` class implements this interface and provides a default implementation for logging errors to configured `Wkg` logging providers.

```csharp
// Program.cs
// configure the error handling services
builder.Services.AddErrorHandling();

// WeatherManager.cs
public class WeatherManager(IWeatherService weatherService, IUserService userService, IConversionService converter) : ManagerBase
{
    public Task<ManagerResult<WeatherForecastEntry[]>> GetWeatherForecastEntriesAsync(string? userId) => ErrorSentry.WatchAsync(async () =>
    {
        ManagerResult<WeatherForecast> result = await GetWeatherForecastAsync(userId);
        if (!result.TryGetResult(out WeatherForecast? weatherForecast))
        {
            // preserve the status code and message of the original result, but re-bind the generic type
            return result.FailureAs<WeatherForecastEntry[]>();
        }
        return Success(weatherForecast.Entries);
    });
}
```

In this example, the `GetWeatherForecastEntriesAsync` method uses the `ErrorSentry.WatchAsync` method to intercept any exceptions that occur during the execution of the method. The default implementation of `ErrorSentry` logs any exceptions to the configured logging providers, allowing developers to track and diagnose errors that occur during the execution of their applications.

### `Transactions` Namespace

`Wkg.AspNetCore.Transactions` provides a set of abstractions and implementations for managing database transactions in ASP\.NET Core applications, alleviating the need to manually manage transaction scopes. Transactions are automatically created and managed on a per-request basis, ensuring that all database operations within a single HTTP request are executed within the same transaction scope, flowing across all manager classes and services. All database transactions managed by the `Wkg.AspNetCore.Transactions` framework support mocking and testing, allowing for easy unit and integration testing of database operations without the need to clutter your components with conditional logic to disable transactions during testing.

#### Configuration

The `Wkg.AspNetCore.Transactions` namespace provides the `AddTransactionManagement<TDbContext>` extension method, which configures the necessary services to enable transaction management for the specified `DbContext` type.

```csharp
// Program.cs
builder.Services.AddTransactionManagement<WeatherDbContext>(options => options
    .UseIsolationLevel(IsolationLevel.ReadCommitted));
```

#### Using Transactions

In order to consume transactions, `Wkg.AspNetCore` provides additional abstractions and implementations for manager classes and services. The `DatabaseManager<TDbContext>` class serves as a convenient base class for manager classes that interact with a database context, allowing them to automatically participate in the transaction scope created for the current HTTP request. Similar base classes are provided for controllers and Razor pages.

```csharp
// UserPreferenceManager.cs
public class UserPreferenceManager(ITransactionServiceHandle transactionService, IUserService userService) : DatabaseManager<WeatherDbContext>
{
    public Task<ManagerResult> UpdateLocationAsync(string userId, Location newLocation) => Transaction.Scoped.RunAsync(async (dbContext, transaction) =>
    {
        User? user = await userService.GetUserByIdAsync(userId);
        if (user is null)
        {
            return transaction.Rollback(NotFound("User not found."));
        }
        user.Location = newLocation;
        await dbContext.SaveChangesAsync();
        return transaction.Commit(Success());
    });

    public Task<ManagerResult<Location>> GetLocationAsync(string userId) => Transaction.Scoped.RunReadOnlyAsync(async dbContext =>
    {
        User? user = await userService.GetUserByIdAsync(userId);
        if (user is null)
        {
            return NotFound("User not found.");
        }
        return Success(user.Location);
    });
}
```

In this example, the `UserPreferenceManager` class inherits from `DatabaseManager<WeatherDbContext>` for convenient access to the transaction scope and database context. Note how the database context is not consumed directly, but is instead provided through an `ITransactionService<WeatherDbContext>` instance, exposed by the `Transaction` property of the `DatabaseManager` base class. This indirection prevents misuse of the database context and ensures that all database operations are executed within the same transaction scope.

The `UpdateLocationAsync` method uses the `RunAsync` method of the default request-scoped `ITransaction<WeatherDbContext>` instance, exposed through the `Scoped` property, to lazily create a new transaction on the underlying database context, if one does not already exist. An `IScopedTransaction` instance is subsequently created, representing the scope in which the lambda expression is executed. Every lambda running in a scoped transaction is expected to return an `IDeferredTransactionState`, which is used to control the transaction's outcome. In read/write scenarios, these outcomes must be explicitly defined by returning either a `Commit` or `Rollback` result, which can be created from the `IScopedTransaction` instance. As `ITransaction<TDbContext>` instances may outlive the scope of a single lambda expression, deferred transaction states are used to control the transaction's outcome. As such, the `Commit` and `Rollback` results from the example above are not immediately applied to the transaction, but are instead deferred until the transaction scope is disposed. This allows subsequent operations on the same transaction to influence and override the transaction's outcome, in accordance with the priorities of the returned deferred transaction states.

The `GetLocationAsync` method uses the `RunReadOnlyAsync` method of the transaction instance to operate in a read-only context, where no changes are expected to be made to the database, preferring to roll back the transaction if necessary, but allowing higher-priority read/write operations to override this behavior.

> :warning: **Caution**
> The `RunReadOnlyAsync` method only specifies that the current lambda expression does not require a defined outcome for the transaction, as it is intended to be read-only. However, no guarantees are made about the final outcome of the transaction, as higher-priority read/write operations may still commit the transaction. As such, read-only operations should not rely on the transaction being rolled back, but should instead focus on returning the desired result. Therefore common sense is recommended when using this method, as performing write operations in a read-only context may lead to unexpected results, based on side effects from other operations on the same transaction.

Once the transaction scope is disposed, the transaction's outcome is determined by the deferred transaction states returned by all lambda expressions executed within the scope. The transaction is then committed or rolled back based on the aggregate transaction state. This aggregate `TransactionState`, exposed through the `State` property of an `ITransaction<TDbContext>` instance, can consist of one or multiple of the following flags, where higher-priority flags override lower-priority flags. Every deferred transaction state corresponds to one of these flags:

| Flag Value | Name | Description |
| --- | --- | --- |
| 0 | `ReadOnly` | Unless otherwise specified, the transaction should be rolled back as no changes are expected to be made, yielding to higher priority states as necessary. As such, read-only operations may result in a commit if a different lambda expression requests it. When combined with other deferred transaction states, this flag has no effect on the final outcome. |
| 1 | `Commit` | The transaction should be committed, yielding to higher priority rollbacks as necessary. |
| 2 | `Rollback` | The transaction should be rolled back, overriding any requests to commit the transaction. This is the highest priority flag that can be set by user code. |
| 6 | `Exception` | Implies `Rollback`. An unhandled exception occurred in user code during the transaction. The transaction will be rolled back, regardless of states specified by user code. |
| 8 | `Finalized` | The underlying transaction was executed against the database with the specified final flags and cannot be further modified. |

> :bulb: **Tip**: Due to the deferred nature of transaction states, recursive calls to `RunAsync` or `RunReadOnlyAsync` are supported, allowing for nested transaction scopes. As always, the final outcome of the transaction is determined by the highest-priority deferred transaction state returned by all lambda expressions executed within the scope.

#### Consuming `ITransactionService` Directly

While the provided database manager, controller, and page base classes usually suffice for most scenarios, there may be cases where transactions need to be consumed directly in services or other classes. In such cases, the `ITransactionService<TDbContext>` interface can also be directly injected into theses classes, allowing them to create and manage transactions as needed.

```csharp
// WeatherService.cs
public class WeatherService(ITransactionService<WeatherDbContext> transaction)
{
    public Task<WeatherForecast> GetWeatherForecastAsync(string location) => transaction.Scoped.RunReadOnlyAsync(async dbContext =>
    {
        return await dbContext.WeatherForecasts.FirstOrDefaultAsync(f => f.Location == location);
    });
}
```

#### Creating Custom Transaction Scopes

In some cases, it may be necessary to create custom transaction scopes that can be manually managed. The `ITransactionService` interface provides this functionality through the `BeginTransaction` method, which creates a new database context and transaction scope that can be used independently of the shared transaction scope created for the current HTTP request. This allows for more fine-grained control over transaction lifetimes and scopes, as well as for the creation of parallel transactions. This can be useful in scenarios where multiple transactions need to be executed concurrently, where transactions need to be managed outside of the context of a single HTTP request, or where transactions need to be explicitly committed or rolled back independently of the shared transaction scope.
    
```csharp
// ErrorLoggingService.cs
public class ErrorLoggingService(ITransactionServiceHandle transactionSeviceHandle) : DatabaseService<WeatherDbContext>(transactionSeviceHandle)
{
    public async Task LogErrorAsync(string message)
    {
        // create a new transaction scope for logging the error and commit it independently of the primary transaction scope
        await using ITransaction<WeatherDbContext> manualTransaction = Transaction.BeginTransaction();
        await transaction.RunAsync(async (dbContext, transaction) => 
        {
            await dbContext.Errors.AddAsync(new Error { Message = message });
            await dbContext.SaveChangesAsync();
            return transaction.Commit();
        });
    }
}
```

In this example, the `ErrorLoggingService` class uses the `BeginTransaction` method of the `ITransactionService` interface to create a new transaction scope for logging errors. This allows the service to manage the transaction independently of the shared transaction scope created for the current HTTP request, ensuring that the error is logged even if the primary transaction scope is rolled back. The `RunAsync` method is then used to execute the logging operation within the new transaction scope, committing the transaction once the operation is complete.

> :warning: **Caution**
> Ensure that custom transaction scopes are always properly disposed to prevent resource leaks and to ensure that the transaction is actually completed against the database. Failing to dispose a transaction scope may result in the transaction being rolled back regardless of the desired outcome.

> :warning: **Caution** 
> In most cases, it is recommended to use the shared transaction scope created for the current HTTP request to ensure consistency and integrity across all database operations. Custom transaction scopes should only be used when absolutely necessary, as they reqire the creation of a new dependency injection scope.

### `Validation` Namespace

The `Validation` namespace provides additional data validation attributes based on the [`Wkg.Data.Validation`](https://git.wkg.lan/WKG/components/wkg-base/-/blob/main/docs/documentation.md#wkgdatavalidation-namespace) framework, providing more refined validation than the built-in data annotations. These attributes can be used to validate input data in ASP\.NET Core applications, ensuring that only valid data is accepted by the application.

| Attribute | Description |
| --- | --- |
| `ValidEmailAddressAttribute` | Specifies that a data field value must be a valid email address conforming to the email address format specified in RFC 5322. |
| `ValidPhoneNumberAttribute` | Specifies that a data field value must be a valid phone number. |
| `ValidUrlAttribute` | Specifies that a data field value must be a valid HTTP, HTTPS, or FTP URL as defined by RFC 3986. |

## `Wkg.AspNetCore.TestAdapters`

The `Wkg.AspNetCore.TestAdapters` library provides a set of utilities and abstractions for testing ASP\.NET Core applications, allowing developers to easily write unit and integration tests for their applications. The library integrates with the `Wkg.AspNetCore` library to provide additional testing capabilities, such as mocking database transactions and providing DI facilities for testing manager classes and services.

> :warning: **Caution**
> The `Wkg.AspNetCore.TestAdapters` library has an implicit runtime dependency on the `Wkg.AspNetCore` library and should be used in conjunction with it. Ensure to reference the identical versions of both libraries to prevent compatibility issues.

### Setup

To ensure that your project integrates with unit testing easily and to test under realistic conditions, identical dependency injection configurations should be for both the main application and the test project, only mocking services as needed. To reuse the same dependency injection configuration, it is recommended to move the DI configuration of your ASP\.NET Core application to a dedicated `Startup` class, implementing the `IStartup` interface, which can be used by both the main application and the test project.

```csharp
// Program.cs
WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
// configure ASPNETCORE_ENVIRONMENT fallbacks and WKG logging
...
// invoke startup configuration
builder.BuildUsing<Startup>().Run();

// Startup.cs
public class Startup : IStartupScript
{
    public static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        // configure dependency injection services
    }

    public static void Configure(WebApplication app)
    {
        // configure middleware
    }
}
```

When using DI in your test project, you can reuse the same `Startup` class to configure the DI container for your tests. This ensures that the same services are registered in both the main application and the test project, allowing you to test your application under realistic conditions.

```csharp
// WeatherTestInitializer.cs
public class WeatherTestInitializer : IDITestInitializer
{
    public static void Configure(IServiceCollection services)
    {
        // run in unit test environment
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "unit-test");
        // re-use the same dependency injection configuration as the main application
        services.ConfigureUsing<Startup>();
        // override services as needed
        services.AddSingleton<IMyService, MockMyService>();
    }

    public static void Initialize(IServiceProvider serviceProvider)
    {
        // initialize services as needed, such as loading mock data
    }
}
```

In this example, the `WeatherTestInitializer` class configures the DI container for the test project by reusing the same `Startup` class used by the main application. The `Configure` method configures the DI container by invoking the `ConfigureServices` method of the `Startup` class, ensuring that the same services are registered in both the main application and the test project, subsequently mocking database transactions for the `WeatherDbContext`. The `Initialize` method initializes any services needed for the tests, such as loading test data into the database.

### Writing Tests

All test classes should inherit from the `TestBase` class provided by the `Wkg.AspNetCore.TestAdapters` library, which initializes internal utilities required for testing and verifies that the library versions are compatible. 

Depending of the components being tested, additional base classes are provided.

#### Testing Simple Classes

For testing simple classes that do not require DI or database transactions, the `TestBase` class is sufficient.

Write tests as usual, using the `TestBase` class as the base class for your test classes:

```csharp
// MySimpleClassTests.cs
[TestClass]
public class MySimpleClassTests : TestBase
{
    [TestMethod]
    public void TestMethod()
    {
        // arrange
        MySimpleClass mySimpleClass = new MySimpleClass();
        // act
        mySimpleClass.DoSomething();
        // assert
        Assert.True(mySimpleClass.SomethingHappened);
    }
}
```

#### Testing Components Using DI

For testing components that require DI, such as manager classes, controllers, or other DI services, the `ComponentTest<TComponent, TInitializer>` class should be used. This class provides additional utilities for instantiating the component under test and managing the DI container. The `TComponent` type parameter specifies the type of the component being tested, while the `TInitializer` type parameter specifies the `IDITestInitializer` implementation to use for configuring the DI container, as detailed in the [Setup](#setup) section.

The following example demonstrates how to write a test for a manager class using the `ComponentTest<TComponent, TInitializer>` class:

```csharp
// MySerializationService.cs
public class MySerializationService(ILogger<MySerializationService> logger)
{
    public string Serialize(object obj)
    {
        logger.LogInformation("Serializing object: {0}", obj);
        return JsonSerializer.Serialize(obj);
    }
}

// MySerializationServiceTests.cs
[TestClass]
public class MySerializationServiceTests : ComponentTest<MySerializationService, WeatherTestInitializer>
{
    [TestMethod]
    public Task TestSerialize() => UsingComponentAsync(myService =>
    {
        // arrange
        object obj = new { Name = "John Doe", Age = 42 };
        // act
        string json = myService.Serialize(obj);
        // assert
        Assert.AreEqual("{\"Name\":\"John Doe\",\"Age\":42}", json);
    });
}
```

In this example, the `MySerializationServiceTests` class uses the `UsingComponentAsync` method provided by the `ComponentTest<TComponent, TInitializer>` class to instantiate the `MySerializationService` class with all its dependencies resolved through an automatically configured DI container scoped to the test method. The test method then arranges the input data, calls the method under test, and asserts the expected outcome.

#### Testing Database Access

When writing tests that interact with a database, such as integration tests for manager classes or services that use database transactions, it is important to ensure that the database is properly initialized with test data and that any changes made during the test are rolled back to prevent side effects on other tests. The `DatabaseTest<TDbContext, TInitializer>` class provides utilities for initializing the database with test data and rolling back changes made during the test.

`Wkg.AspNetCore.TestAdapters` ensures this by providing two extension methods that can be used during DI configuration in the test project.

```csharp
// WeatherTestInitializer.cs
public class WeatherTestInitializer : IDITestInitializer
{
    public static void Configure(IServiceCollection services)
    {
        // run in unit test environment
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "unit-test");
        // re-use the same dependency injection configuration as the main application
        services.ConfigureUsing<Startup>();
        // override services as needed
        services.MockDatabaseTransactions<WeatherDbContext>();
    }

    public static void Initialize(IServiceProvider serviceProvider)
    {
        // initialize services as needed
        // load test data
        serviceProvider.InitializeTestDatabase<WeatherTestDatabaseLoader>();
    }
}
```

The `MockDatabaseTransactions<TDbContext>` extension method configures the `Wkg.AspNetCore.Transactions` framework to roll back all all `ITransaction<TDbContext>` instances created during the test, regardless of their outcome, allowing you to write your service code as usual without having to add additional logic to disable transactions when your components are being tested. Read more about transactions in the [Transactions](#transactions-namespace) section.

The `InitializeTestDatabase<ITestDatabaseLoader>` extension method initializes the database with test data using the specified `ITestDatabaseLoader` implementation. This allows you to load test data into the database before running your tests, ensuring that the database is in a known state before the tests are executed. The test database itself may be automatically created and migrated to the latest schema version using external infrastructure, such as Docker containers and GitLab CI/CD pipeline stages.

Subsequently, write tests for your components as usual, and DI will take care of the rest:

```csharp
// UserManagerTests.cs
[TestClass]
public class UserManagerTests : ComponentTest<WeatherDbContext, WeatherTestInitializer>
{
    [TestMethod]
    public Task TestCreateUser() => UsingComponentAsync(async userManager =>
    {
        // arrange
        User user = new User { Name = "John Doe", Email = "johndoe@example.com" };
        // act
        ManagerResult result = await userManager.CreateUserAsync(user);
        // assert
        Assert.IsTrue(result.IsSuccess);
        // ensure the user was created in the database
        User? createdUser = await userManager.GetUserByEmailAsync("johndoe@example.com");
        Assert.IsNotNull(createdUser);
        Assert.AreEqual("John Doe", createdUser.Name);
    });
}
```

In this example, the `TestCreateUser` method creates a new user using the `UserManager` class and asserts that the user was successfully created in the database. Since the `UserManager` class uses `Wkg.AspNetCore.Transactions` to manage database transactions, subsequent read operations in the test method will automatically participate in the same transaction scope, ensuring that the data was correctly persisted to the database. After the test method completes, the DI scope is disposed, and the `ITransaction<WeatherDbContext>` instance is finalized, rolling back any changes made during the test, as previously configured by the `MockDatabaseTransactions<WeatherDbContext>` extension method.

##### Database Loaders

To load test data into the database before running any tests, you can implement a custom test database loader by inheriting from the `TestDatabaseLoaderBase<TSelf, TDbContext>` class and implementing the `ITestDatabaseLoader<TDbContext>` interface.

```csharp
// WeatherTestDatabaseLoader.cs
class WeatherTestDatabaseLoader : TestDatabaseLoaderBase<WeatherTestDatabaseLoader, WeatherDbContext>, ITestDatabaseLoader<WeatherDbContext>
{
    public void InitializeDatabase(WeatherDbContext dbContext)
    {
        dbContext.Set<User>().AddRange
        (
            new User { Name = "Alice", Email = "alice@example.com" },
            new User { Name = "Bob", Email = "bob@example.com "}
        );
        ...
        dbContext.SaveChanges();
    }
}
```

##### Using the DbContext Directly in Tests

In some cases, you may need to interact with the database context directly in your tests. The `DbContextTest<TDbContext, TInitializer>` class provides this functionality by allowing you to access the database context directly in your test methods. This can be useful for writing integration tests that require direct access to the database context, such as testing complex queries, database operations that are not covered by the manager classes, or testing the behavior of the database context itself.

```csharp
// WeatherDbContextTests.cs
[TestClass]
public class WeatherDbContextTests : DbContextTest<WeatherDbContext, WeatherTestInitializer>
{
    [TestMethod]
    public Task TestSoftDelete() => UsingDbContextAsync(async dbContext =>
    {
        User? bob = await dbContext.Set<User>()
            .FirstOrDefaultAsync(u => u.Email == "bob@example.com");
        Assert.IsNotNull(bob);
        await dbContext.Set<User>().RemoveAsync(bob);
        await dbContext.SaveChangesAsync();
        bob = await dbContext.Users
            .FirstOrDefaultAsync(u => u.Email == "bob@example.com");
        Assert.IsNull(bob);
        bob = await dbContext.Set<User>()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Email == "bob@example.com");
        Assert.IsNotNull(bob);
        Assert.IsTrue(bob.IsDeleted);
    });
}
```

In this example, the `TestSoftDelete` method uses the `UsingDbContextAsync` method provided by the `DbContextTest<TDbContext, TInitializer>` class to access the `WeatherDbContext` directly in the test method. The test then ensures that the soft delete functionality of the `User` entity is working correctly by removing a user from the database and verifying that the user is marked as deleted in the database context. After the test method completes, the changes made to the database context are rolled back, ensuring that the database is in a known state before the next test is executed.