# GridifyGlobalConfiguration

Using this class you can change the default behavior and configuration of the gridify library.

## General configurations

### DefaultPageSize

The default page size for the paging methods when no page size is specified.

- type: `int`
- default: `20`

### CaseSensitiveMapper

By default mappings are case insensitive. for example, `name=John` and `Name=John` are considered equal.
You can change this behavior by setting this property to `true`.

- type: `bool`
- default: `false`
- related to: [GridifyMapper - CaseSensitive](./gridifyMapper.md#casesensitive)

### AllowNullSearch

This option enables the 'null' keyword in filtering operations, for example, `name=null` searches for all records with a null value for the `name` field not the string `"null"`. if you need to search for the string `"null"` you can

- type: `bool`
- default: `true`
- related to: [GridifyMapper - AllowNullSearch](./gridifyMapper.md#allownullsearch)

### IgnoreNotMappedFields

If true, in filtering and ordering operations, gridify doesn't return any exceptions when a mapping is not defined for the given field.

- type: `bool`
- default: `false`
- related to: [GridifyMapper - IgnoreNotMappedFields](./gridifyMapper.md#ignorenotmappedfields)

## CustomOperators

Sometimes the default [Gridify operators](./filtering.md#conditional-operators) are not enough, For example, when you are using the EntityFramework, you may want to use `EF.Functions.FreeText` rather than a LIKE with wildcards. In this case, you can register your own custom operator through this property.

To define a custom operator, you need to create a class that implements the `IGridifyOperator` interface.

```csharp
class FreeTextOperator : IGridifyOperator
{
   public string GetOperator() => "#=*";
   public Expression<OperatorParameter> OperatorHandler()
   {
      return (prop, value) => EF.Functions.FreeText(prop, value.ToString());
   }
}
```

Then register it through CustomOperators property.

```csharp
 GridifyGlobalConfiguration.CustomOperators.Register(new FreeTextOperator());
```

::: warning
The custom operator must be started with a `#` character.
:::

## EntityFramework

### EntityFrameworkCompatibilityLayer

Setting this property to `true` Enables the EntityFramework Compatibility layer to make the generated expressions compatible whit entity framework.

- type: `bool`
- default: `false`

::: tip
Lean more about the [compatibility layer](./entity-framework.md#compatibility-layer)
:::

### EnableEntityFrameworkCompatibilityLayer()

Simply sets the [EntityFrameworkCompatibilityLayer](#entityframeworkcompatibilitylayer) property to `true`.

### DisableEntityFrameworkCompatibilityLayer()

Simply sets the [EntityFrameworkCompatibilityLayer](#entityframeworkcompatibilitylayer) property to `false`.

