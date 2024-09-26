# WKG ASP .NET Core

[![NuGet version (Wkg.AspNetCore)](https://img.shields.io/nuget/v/Wkg.AspNetCore.svg?style=flat-square)](https://www.nuget.org/packages/Wkg.AspNetCore/) [![NuGet version (Wkg.AspNetCore.TestAdapters)](https://img.shields.io/nuget/v/Wkg.AspNetCore.TestAdapters.svg?style=flat-square)](https://www.nuget.org/packages/Wkg.AspNetCore.TestAdapters/)

---

`Wkg.AspNetCore` is a company-internal library providing reusable components for the development of ASP .NET Core applications at WKG. It provides new abstractions over existing ASP .NET Core components, provides request-scoped database transactions, new authentication options, data validation, and more.

A core goal of `Wkg.AspNetCore` is to simplify the development of ASP .NET Core applications by providing a set of reusable components that can be easily integrated into new or existing projects, allowing for consistent and maintainable codebases and easier testing with the `Wkg.AspNetCore.TestAdapters` library that can also be found in this repository.

As part of our commitment to open-source software, we are making this library available to the public under the GNU General Public License v3.0. We hope that it will be useful to other developers and that the community will contribute to its further development.

## Installation

Install the `Wkg.AspNetCore` NuGet package by adding the following package reference to your project file:

```xml
<ItemGroup>
    <PackageReference Include="Wkg.AspNetCore" Version="X.X.X" />
</ItemGroup>
```

> :warning: **Warning**
> Replace `X.X.X` with the latest stable version available on the [nuget feed](https://www.nuget.org/packages/Wkg.AspNetCore), where **the major version must match the major version of your targeted .NET runtime**.

> :warning: **Warning**
> To use the `Wkg.AspNetCore.TestAdapters` library in your test projects, make sure that the version of the `Wkg.AspNetCore.TestAdapters` package exactly matches the version of the `Wkg.AspNetCore` package that you are using in your main project.

## Usage

For technical documentation and usage examples, please refer to the [documentation](https://github.com/WKG-Software-GmbH/wkg-aspnet-core/blob/main/docs/documentation.md).