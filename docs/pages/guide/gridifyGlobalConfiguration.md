# GridifyGlobalConfiguration

Using this class, you can change the default behavior and configuration of the Gridify library.

## General configurations

### DefaultPageSize

The default page size for the paging methods when no page size is specified.

- type: `int`
- default: `20`

### CaseSensitiveMapper

By default mappings are case insensitive. For example, `name=John` and `Name=John` are considered equal.
You can change this behavior by setting this property to `true`.

- type: `bool`
- default: `false`
- related to: [GridifyMapper - CaseSensitive](./gridifyMapper.md#casesensitive)

### AllowNullSearch

This option enables the `null` keyword in filtering operations. For example, `name=null` searches for all records with a null value for the `name` field, not the string `"null"`. If you need to search for the literal string `"null"`, disable this option.

- type: `bool`
- default: `true`
- related to: [GridifyMapper - AllowNullSearch](./gridifyMapper.md#allownullsearch)

### IgnoreNotMappedFields

If true, Gridify doesn't throw exceptions during filtering and ordering operations when a mapping is not defined for the given field.

- type: `bool`
- default: `false`
- related to: [GridifyMapper - IgnoreNotMappedFields](./gridifyMapper.md#ignorenotmappedfields)

### DisableNullChecks

By default, Gridify adds null-checking conditions on nested collections to prevent null reference exceptions, e.g., `() => field != null && field....`

Some ORMs like NHibernate don't support this. You can disable this behavior by setting this option to `true`.

- type: `bool`
- default: `false`

### AvoidNullReference

This option allows intermediate objects to be null.

For example, in `obj.PropA.PropB`, `PropA` can be null.

This configuration is specific to properties and was introduced after `DisableNullChecks`.

- type: `bool`
- default: `false`

### CaseInsensitiveFiltering

If true, string comparison operations are case insensitive by default.

- type: `bool`
- default: `false`

### DefaultDateTimeKind

By default, Gridify uses the `DateTimeKind.Unspecified` when parsing dates. You can change this behavior by setting this property to `DateTimeKind.Utc` or `DateTimeKind.Local`. This option is useful when you want to use Gridify with a database that requires a specific `DateTimeKind`, for example when using npgsql and postgresql. 

- type: `DateTimeKind`
- default: `null`

## CustomOperators

Using the `Register` method of this property you can add your own custom operators.

``` csharp
 GridifyGlobalConfiguration.CustomOperators.Register<MyCustomOperator>();
```

To learn more about custom operators, see [Custom operator](./filtering.md#custom-operators)
