
# Getting Started

There are three packages available for gridify in the nuget repository.

- [Gridify](https://www.nuget.org/packages/Gridify/)
- [Gridify.EntityFramework](https://www.nuget.org/packages/Gridify.EntityFramework/)
- [Gridify.Elasticsearch](https://www.nuget.org/packages/Gridify.Elasticsearch/)

::: tip
If you are using the Entity framework in your project, you should install the `Gridify.EntityFramework` package instead of `Gridify`.

This package has the same functionality as the `Gridify` package, but it is designed to be more compatible with [Entity Framework](./extensions/entityframework).

In order to use Gridify with Elasticsearch it's necessary to install `Gridify.Elasticsearch`. Please read [the separate thread of the documentation](./extensions/elasticsearch).
:::

## Installation

### Package Manager

```shell-vue
Install-Package Gridify -Version {{ $version }}
```

### .NET CLI

```shell-vue
dotnet add package Gridify --version {{ $version }}
```

## Namespace

After installing the package, you can use the `Gridify` namespace to access the package classes and static Extension methods.

``` csharp
using Gridify;
```

## How to use

There are two ways to start using Gridify:

- Using the [Extension](./extensions.md) methods
- Using [QueryBuilder](./queryBuilder.md)
