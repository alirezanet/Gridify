
# Getting Started

There are three packages available for gridify in the nuget repository.

- [Gridify](https://www.nuget.org/packages/Gridify/)
- [Gridify.EntityFramework](https://www.nuget.org/packages/Gridify.EntityFramework/)

::: tip
If you are using the Entity framework in your project, you should install the `Gridify.EntityFramework` package instead of `Gridify`.

This package has the same functionality as the `Gridify` package, but it is designed to be more compatible with [Entity Framework](./extensions/entityframework).
:::

---
- [Gridify.Elasticsearch](https://www.nuget.org/packages/Gridify.Elasticsearch/)

::: tip
In order to use Gridify with Elasticsearch it's necessary to install [`Gridify.Elasticsearch`](./extensions/elasticsearch).
:::

---

If you're developing in a JavaScript or TypeScript environment and need to create dynamic queries on the client side for server-side operations,
Gridify also offer a lightweight client library. [gridify-client](./extensions/gridify-client)

- [Gridify Client (JS/TS)](https://www.npmjs.com/package/gridify-client)

## Installation

:::: code-group

```shell-vue [Gridify]
dotnet add package Gridify --version {{ $version }}
```

```shell-vue [Gridify.EntityFramework]
dotnet add package Gridify.EntityFramework --version {{ $version }}
```

```shell-vue [Gridify.Elasticsearch]
dotnet add package Gridify.Elasticsearch --version {{ $version }}
```

```shell-vue [Gridify Client (JS/TS)]
npm i gridify-client
```

::::

## Namespace

After installing the package, you can use the `Gridify` namespace to access the package classes and static Extension methods.


``` csharp
using Gridify;
```

## How to use

There are two ways to start using Gridify:

- Using the [Extension](./extensions.md) methods
- Using [QueryBuilder](./queryBuilder.md)
