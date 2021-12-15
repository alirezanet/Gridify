# Entity Framework

## Gridify.EntityFramework Package

The `Gridify.EntityFramework` package has two additional `GridifyAsync()` and `GridifyQueryableAsync()` methods.

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
