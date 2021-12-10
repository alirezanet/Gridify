<script>
   export default {
      setup() {
         return {
            version: '2.4.7'
         }
      }
   }
</script>

# Getting Started

There are two packages available for gridify in the nuget repository.

- [Gridify](https://www.nuget.org/packages/Gridify/)
- [Gridify.EntityFrmamework](https://www.nuget.org/packages/Gridify.EntityFramework/)

::: tip
If you are using the Entity framework in your project, you should install the `Gridify.EntityFramework` package instead of `Gridify`.

This package has the same functionality as the `Gridify` package, but it is designed to be more compatible with [Entity Framework](./entity-framework.md).
:::


## Installation

### Package Manager
``` pm:no-line-numbers:no-v-pre
Install-Package Gridify -Version {{ version }}
```

``` pm:no-line-numbers:no-v-pre
Install-Package Gridify.EnitityFramework -Version {{ version }}
```

### .NET CLI
``` cmd:no-line-numbers:no-v-pre
dotnet add package Gridify --version {{ version }}
```
``` cmd:no-line-numbers:no-v-pre
dotnet add package Gridify.EntityFramework --version {{ version }}
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
- Using [QueryBuilder](./querybuilder.md)
