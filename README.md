# WKG ASP .NET Core

![](https://git.wkg.lan/WKG/components/wkg-aspnet-core/badges/main/pipeline.svg)

---

`Wkg.AspNetCore` is a company-internal library providing reusable components for the development of ASP .NET Core applications at WKG. It provides new abstractions over existing ASP .NET Core components, provides request-scoped database transactions, new authentication options, data validation, and more.

A core goal of `Wkg.AspNetCore` is to simplify the development of ASP .NET Core applications by providing a set of reusable components that can be easily integrated into new or existing projects, allowing for consistent and maintainable codebases and easier testing with the `Wkg.AspNetCore.TestAdapters` library that can also be found in this repository.

## Installation

The `Wkg.AspNetCore` library is available as a NuGet package from our internal nuget feed. To install it, add the following package source to your NuGet configuration:

```xml
<PropertyGroup>
    <RestoreAdditionalProjectSources>
        https://baget.wkg.lan/v3/index.json
    </RestoreAdditionalProjectSources>
</PropertyGroup>
```

Then, install the package by adding the following package reference to your project file:

```xml
<ItemGroup>
    <PackageReference Include="Wkg.AspNetCore" Version="X.X.X" />
</ItemGroup>
```

> :warning: **Warning**
> Replace `X.X.X` with the latest stable version available on the [nuget feed](https://baget.wkg.lan/packages/wkg/latest), where **the major version must match the major version of your targeted .NET runtime**.

## Usage

For technical documentation and usage examples, please refer to the [documentation](/docs/documentation.md).