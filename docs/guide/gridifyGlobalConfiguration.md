!!!include(./docs/guide/snippets/gridifyGlobalConfiguration.md)!!!

## CustomOperators

Using the `Register` method of this property you can add your own custom operators.

``` csharp
 GridifyGlobalConfiguration.CustomOperators.Register(new MyCustomOperator());
```

To learn more about custom operators, see [Custom operator](./filtering.md#custom-operators)

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
