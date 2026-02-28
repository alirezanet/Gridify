# Entity Framework

## Gridify.EntityFramework Package

The `Gridify.EntityFramework` package has two additional `GridifyAsync()` and `GridifyQueryableAsync()` methods.

## Installation

### Package Manager

```shell-vue
Install-Package Gridify.EntityFramework -Version {{ $version }}
```

### .NET CLI

```shell-vue
dotnet add package Gridify.EntityFramework --version {{ $version }}
```

## Compatibility layer

If you want to use gridify with Entity Framework, you should enable the compatibility layer:

``` csharp
GridifyGlobalConfiguration.EnableEntityFrameworkCompatibilityLayer();
```

Enabling the compatibility layer provides the following features:

- It tweaks the internal expression builder to make it compatible with Entity Framework.
- EF query optimization
- EF ServiceProviderCaching support
- Creates parameterized queries

``` sql
DECLARE @__Value_0 nvarchar(4000) = N'vahid';

SELECT [u].[Id], [u].[CreateDate], [u].[FkGuid], [u].[Name]
FROM [Users] AS [u]
WHERE [u].[Name] = @__Value_0
```

## Configuration

### EntityFrameworkCompatibilityLayer

Setting this property to `true` Enables the EntityFramework Compatibility layer to make the generated expressions compatible with entity framework.

- type: `bool`
- default: `false`

### EnableEntityFrameworkCompatibilityLayer()

Simply sets the [EntityFrameworkCompatibilityLayer](#entityframeworkcompatibilitylayer) property to `true`.

### DisableEntityFrameworkCompatibilityLayer()

Simply sets the [EntityFrameworkCompatibilityLayer](#entityframeworkcompatibilitylayer) property to `false`.

## Composite Maps Compatibility

When using [composite maps](../gridifyMapper#addcompositemap) with Entity Framework (especially PostgreSQL), follow these guidelines for optimal compatibility:

### Working with Different Data Types

::: warning Guid Properties
```csharp
// ✅ Recommended for EF
mapper.AddCompositeMap("search", 
    x => x.FkGuid.ToString(),  // Convert to string
    x => x.Name);

// ❌ Not recommended for EF (may cause translation issues)
mapper.AddCompositeMap("search", 
    x => (object)x.FkGuid,
    x => x.Name);
```
:::

::: warning Numeric Properties
```csharp
// ✅ Recommended for EF
mapper.AddCompositeMap("search", 
    x => x.Name,
    x => x.Id.ToString());  // Convert to string

// ❌ Not recommended for EF (may cause type mismatch)
mapper.AddCompositeMap("search", 
    x => x.Name,
    x => (object)x.Id);
```
:::

::: warning DateTime Properties (PostgreSQL)
```csharp
// ✅ Required for PostgreSQL
var mapper = new GridifyMapper<User>(cfg => cfg.DefaultDateTimeKind = DateTimeKind.Utc)
    .AddCompositeMap("search", 
        x => x.Name,
        x => (object)x.CreateDate);

// ❌ Without UTC configuration, PostgreSQL will throw an error
```
:::

### Best Practices

1. **Use `.ToString()` for type consistency** - When combining different property types (string, numeric, Guid), convert them all to strings
2. **Set UTC DateTimeKind for PostgreSQL** - Configure `DefaultDateTimeKind = DateTimeKind.Utc` when working with DateTime properties
3. **Test with `.ToQueryString()`** - Verify EF can translate your expressions to SQL:
   ```csharp
   var sql = dbContext.Users
       .ApplyFiltering("search=value", mapper)
       .ToQueryString();
   ```
4. **Keep types homogeneous** - When possible, map properties of the same type together for better performance

