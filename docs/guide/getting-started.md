# Getting Started

There are three packages available for gridify in the nuget repository.

- [Gridify](https://www.nuget.org/packages/Gridify/)
- [Gridify.EntityFramework](https://www.nuget.org/packages/Gridify.EntityFramework/)
- [Gridify.Elasticsearch](https://www.nuget.org/packages/Gridify.Elasticsearch/)

::: tip
If you are using the Entity framework in your project, you should install the `Gridify.EntityFramework` package instead of `Gridify`.

This package has the same functionality as the `Gridify` package, but it is designed to be more compatible with [Entity Framework](./entity-framework.md).

In order to use Gridify with Elasticsearch it's necessary to install `Gridify.Elasticsearch`. Please
read [the separate thread of the documentation](./elasticsearch/getting-started.md).

[Gridify.Abstraction](abstractions.md) includes a set of interface and classes that act like contract with other libraries to use it without referencing gridify package itself.
:::

## Installation

### Package Manager

``` pm:no-line-numbers:no-v-pre
Install-Package Gridify -Version {{ $version }}
```

``` pm:no-line-numbers:no-v-pre
Install-Package Gridify.EntityFramework -Version {{ $version }}
```

### .NET CLI

``` cmd:no-line-numbers:no-v-pre
dotnet add package Gridify --version {{ $version }}
```

``` cmd:no-line-numbers:no-v-pre
dotnet add package Gridify.EntityFramework --version {{ $version }}
```

## Namespace

After installing the package, you can use the `Gridify` namespace to access the package classes and static Extension methods.

``` csharp
using Gridify;
...
```

## How to use

There are two ways to use Gridify:

- Using the [Extension](./extensions.md) methods
- Using [QueryBuilder](./queryBuilder.md)
