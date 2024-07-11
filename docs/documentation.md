# `Wkg.AspNetCore` Documentation

`Wkg.AspNetCore` is a company-internal library providing reusable components for the development of ASP .NET Core applications at WKG.

- [`Wkg.AspNetCore` Documentation](#wkgaspnetcore-documentation)
  - [Components](#components)
    - [`Abstractions` Namespace](#abstractions-namespace)
      - [Introducing Manager Classes](#introducing-manager-classes)
      - [Communicating Results with `ManagerResult`](#communicating-results-with-managerresult)
        - [Converting Between Result Types](#converting-between-result-types)

## Components

### `Abstractions` Namespace

A core part of the `Wkg.AspNetCore` library is the `Abstractions` namespace, which provides new abstractions over existing ASP .NET Core components. These abstractions are designed to extend and unify the existing ASP .NET Core components, such as API controllers, Razor pages, and services.

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
    public WeatherForecast WeatherForecast { get; private set; } = nulL!;

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
    public WeatherForecast WeatherForecast { get; private set; } = nulL!;

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