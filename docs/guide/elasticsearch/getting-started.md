
# Getting Started

In order to use Gridify with Elasticsearch it's necessary to install [Gridify.Elasticsearch](https://TBD).

## Installation

### Package Manager

``` pm:no-line-numbers:no-v-pre
Install-Package Gridify.Elasticsearch -Version {{ $version }}
```

### .NET CLI

``` cmd:no-line-numbers:no-v-pre
dotnet add package Gridify.Elasticsearch --version {{ $version }}
```

## Namespace

After installing the package, you can use the `Gridify` namespace to access the package classes and static Extension methods.

``` csharp
using Gridify.Elasticsearch;
...
```

## How to use

There are two ways to use Gridify:

- Using the [Extension](./extensions.md) methods
- Using [QueryBuilder](./queryBuilder.md)
