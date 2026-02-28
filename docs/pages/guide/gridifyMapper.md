# GridifyMapper

Internally Gridify is using an auto generated mapper that maps your string base field names to actual properties in your entities, but sometimes we don't want to support filtering or sorting on a specific field. If you want to control what field names are mapped to what properties, you can create a custom mapper.

To get a better understanding of how this works, consider the following example:

``` csharp
// sample Entities
public class Person
{
    public string UserName { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Password { get; set; }
    public Contact Contact { get; set; }
}

public class Contact
{
    public string Address { get; set; }
    public int PhoneNumber { get; set; }
}
```

In this example we want to:

- Generate default mappings
- Ignore the `Password` property
- Map the `address` and `mobile` to the Contact property
- Make sure the userName value is always lowercase in the search

``` csharp
var mapper = new GridifyMapper<Person>()
            .GenerateMappings()
            .RemoveMap(nameof(Person.Password))
            .AddMap("address", p => p.Contact.Address)
            .AddMap("mobile", p => p.Contact.PhoneNumber)
            .AddMap("userName", p => p.UserName, v => v.ToLower());
```

In the following, we will become more familiar with the above methods

## GenerateMappings

This method generates mappings for the properties of the entity, including top-level public properties and properties of nested classes up to the specified nesting depth.

- To generate mappings for **top-level public properties** only, you can call this method without passing any arguments:

```csharp
var mapper = new GridifyMapper<Person>()
               .GenerateMappings();
```

- To generate mappings with **control over nesting depth**, you can specify the `maxNestingDepth` parameter. This parameter limits how deep the mappings will be generated for nested classes and navigation properties `(added in v2.15.0)`. Set it to 0 for no nesting or a positive value to control the depth `(added in v2.11.0)`:

```csharp
var mapper = new GridifyMapper<Person>()
      // Generates mappings for top-level properties and properties of nested classes up to 2 levels deep.
     .GenerateMappings(2);
```

::: tip
Another alternative to generate default mappings for top-level public properties is by passing true to the GridifyMapper constructor. This generates mappings without considering nesting depth.

``` csharp
var mapper = new GridifyMapper<Person>(true);
var mapperWithDepth = new GridifyMapper<Person>(true, 2);
```

:::

## RemoveMap

This method removes mapping from the mapper. Usually you will use this method after you have generated the mappings to ignore some properties that you don't want to be supported by Gridify filtering or ordering actions.

## AddMap

This method adds a mapping to the mapper.

- the first parameter is the name of the field you want to use in the string query
- the second parameter is a property selector expression
- the third parameter is an optional [value convertor](#value-convertor) expression that you can use to convert user inputs to anything you want

### Value convertor

If you need to change your search values before the filtering operation you can use this feature, the third parameter of the GridifyMapper AddMap method accepts a function that you can use to convert the input values.

in the above example we want to convert the userName value to lowercase before the filtering operation.

``` csharp
mapper = mapper.AddMap("userName", p => p.UserName, value => value.ToLower());
```

## AddCompositeMap

The `AddCompositeMap` method allows you to search across multiple properties with a single filter reference, automatically combining them with OR logic. This eliminates the need to construct complex OR filter strings on the frontend.

### Basic Usage

```csharp
var mapper = new GridifyMapper<Person>()
    .AddCompositeMap("search", 
        x => x.FirstName,
        x => x.LastName,
        x => x.UserName);

// Frontend sends: search=John
// Generates: WHERE FirstName = 'John' OR LastName = 'John' OR UserName = 'John'
```

### With Shared Convertor

You can apply a shared value converter function that transforms filter values before comparison:

```csharp
var mapper = new GridifyMapper<Product>()
    .AddCompositeMap("search", 
        value => value.ToUpper(),  // Shared convertor
        x => x.Name,
        x => x.Description);

// Filter: search=phone
// Converts "phone" to "PHONE" before searching
```

### With Different Property Types

```csharp
// For in-memory collections
var mapper = new GridifyMapper<Product>()
    .AddCompositeMap("search",
        x => x.Name,
        x => x.Description,
        x => (object)x.Id);  // Cast to object for non-string types

// For Entity Framework (recommended)
var mapper = new GridifyMapper<Product>()
    .AddCompositeMap("search",
        x => x.Name,
        x => x.Description,
        x => x.Id.ToString());  // Convert to string for EF compatibility
```

### Method Signatures

```csharp
// Without convertor
IGridifyMapper<T> AddCompositeMap(
    string from,
    params Expression<Func<T, object?>>[] expressions)

// With convertor
IGridifyMapper<T> AddCompositeMap(
    string from,
    Func<string, object>? convertor,
    params Expression<Func<T, object?>>[] expressions)
```

**Parameters:**
- `from`: The field name to use in filters
- `convertor`: Optional shared value converter function
- `expressions`: One or more property expressions to search across

**Returns:** The mapper instance for method chaining

### Supported Operators

Composite maps support all Gridify operators: `=`, `!=`, `>`, `<`, `>=`, `<=`, `=*`, `!*`, `^`, `$`, `!^`, `!$`

### Benefits

- **Cleaner Frontend Code** - Send `search=value` instead of `name=value|email=value|phone=value`
- **Backend Control** - Change searchable fields without frontend changes
- **Type Safety** - Compile-time checking of property expressions

::: warning Entity Framework Users
When using composite maps with Entity Framework, especially with PostgreSQL, follow the [Entity Framework compatibility guidelines](./extensions/entityframework#composite-maps-compatibility) for proper type handling.
:::

## AddNestedMapper

The `AddNestedMapper` method allows you to reuse mapper configurations for nested objects across multiple entities. This is particularly useful when you have the same nested type (like `Address`) used in multiple parent entities (like `User` and `Company`), and you want to define the nested mappings once and reuse them everywhere.

### Basic Usage

```csharp
// Define a reusable mapper for Address
var addressMapper = new GridifyMapper<Address>()
    .AddMap("city", x => x.City)
    .AddMap("country", x => x.Country);
// Note: Secret is intentionally not mapped

// Reuse the address mapper for User
var userMapper = new GridifyMapper<User>()
    .AddMap("email", x => x.Email)
    .AddNestedMapper(x => x.Address, addressMapper);
// Now supports: "address.city=London", "address.country=UK"

// Reuse the same mapper for Company
var companyMapper = new GridifyMapper<Company>()
    .AddMap("name", x => x.Name)
    .AddNestedMapper(x => x.Address, addressMapper);
// Also supports: "address.city=London", "address.country=UK"
```

### With Custom Prefix

You can specify a custom prefix for the nested mappings:

```csharp
var addressMapper = new GridifyMapper<Address>()
    .AddMap("city", x => x.City)
    .AddMap("country", x => x.Country);

var companyMapper = new GridifyMapper<Company>()
    .AddMap("name", x => x.Name)
    .AddNestedMapper("location", x => x.Address, addressMapper);
// Now supports: "location.city=London", "location.country=UK"
```

### Method Signatures

```csharp
// With explicit prefix (recommended for consistency with AddMap)
IGridifyMapper<T> AddNestedMapper<TProperty>(
    string prefix,
    Expression<Func<T, TProperty>> propertyExpression,
    IGridifyMapper<TProperty> nestedMapper,
    bool overrideIfExists = true)

// With automatic prefix generation from property name
IGridifyMapper<T> AddNestedMapper<TProperty>(
    Expression<Func<T, TProperty>> propertyExpression,
    IGridifyMapper<TProperty> nestedMapper,
    bool overrideIfExists = true)
```

**Parameters:**
- `prefix`: Prefix to prepend to nested mapping keys (first overload only)
- `propertyExpression`: Expression pointing to the nested property (e.g., `x => x.Address`)
- `nestedMapper`: The mapper containing mappings for the nested type
- `overrideIfExists`: Whether to override existing mappings with the same key (default: true)

**Returns:** The mapper instance for method chaining

### Features

- **Reusability** - Define nested mappings once, reuse across multiple entities
- **Type Safety** - Compile-time checking of property expressions
- **Convertor Support** - Nested mappings preserve their value convertors
- **Composite Map Support** - Works with composite maps defined in the nested mapper
- **Security** - Only expose fields you explicitly map; unmapped fields remain hidden

### Example: Securing Nested Properties

```csharp
public class Address
{
    public string City { get; set; }
    public string Country { get; set; }
    public string Secret { get; set; }  // Sensitive data
}

// Create a secure address mapper that excludes Secret
var addressMapper = new GridifyMapper<Address>()
    .AddMap("city", x => x.City)
    .AddMap("country", x => x.Country);
    // Secret is intentionally not mapped

// Apply to multiple entities
var userMapper = new GridifyMapper<User>()
    .AddNestedMapper(x => x.Address, addressMapper);

var companyMapper = new GridifyMapper<Company>()
    .AddNestedMapper(x => x.Address, addressMapper);

// Secret is not exposed in any of these mappers
Assert.False(userMapper.HasMap("address.secret"));
Assert.False(companyMapper.HasMap("address.secret"));
```

### Benefits

- **DRY Principle** - Don't repeat yourself; define nested mappings once
- **Consistency** - Ensure the same fields are exposed/hidden across all entities
- **Maintainability** - Change nested mappings in one place, apply everywhere
- **Similar to AutoMapper** - Works like embedded DTO mappings in AutoMapper

## HasMap

This method checks if the mapper has a mapping for the given field name.

## ClearMaps

This method clears the list of mappings

## GetCurrentMaps

This method returns list of current mappings.

## GetCurrentMapsByType

This method returns list of current mappings for the given type.

## GridifyMapperConfiguration

``` csharp
var mapperConfig = new GridifyMapperConfiguration()
{
   CaseSensitive = false,
   AllowNullSearch = true,
   IgnoreNotMappedFields = false
};

var mapper = new GridifyMapper<Person>(mapperConfig);
```

### CaseSensitive

By default mapper is `Case-insensitive` but you can change this behavior if you need `Case-Sensitive` mappings.

- Type: `bool`
- Default: `false`

``` csharp
var mapper = new GridifyMapper<Person>(q => q.CaseSensitive = true);
```

### IgnoreNotMappedFields

By setting this to `true` Gridify don't throw an exception when a field name is not mapped. for instance, in the above example, searching for `password` will not throw an exception.

- Type: `bool`
- Default: `false`

``` csharp
var mapper = new GridifyMapper<Person>(q => q.IgnoreNotMappedFields = true);
```

### AllowNullSearch

By setting this to `false`, Gridify don't allow searching on null values using the `null` keyword for values.

- Type: `bool`
- Default: `true`

``` csharp
var mapper = new GridifyMapper<Person>(q => q.AllowNullSearch = false);
```

### CaseInsensitiveFiltering

If true, string comparison operations are case insensitive by default.

- type: `bool`
- default: `false`

``` csharp
var mapper = new GridifyMapper<Person>(q => q.CaseInsensitiveFiltering = true);
```

### DefaultDateTimeKind

By setting this property to a `DateTimeKind` value, you can change the default `DateTimeKind` used when parsing dates.

- type: `DateTimeKind`
- default: `null`

``` csharp
var mapper = new GridifyMapper<Person>(q => q.DefaultDateTimeKind = DateTimeKind.Utc);
```

Here's the addition for `EntityFrameworkCompatibilityLayer` with slight improvements for clarity:

---

### DisableCollectionNullChecks

This setting is similar to [`DisableNullChecks`](./gridifyGlobalConfiguration.md#disablenullchecks) in the global configuration, but it allows you to enable this setting on a per-query basis instead of globally. When set to true, Gridify won't check for null values in collections during filtering operations.

- **Type:** `bool`
- **Default:** `false`

```csharp
var mapper = new GridifyMapper<Person>(q => q.DisableCollectionNullChecks = true);
```

### AvoidNullReference


This setting is similar to [`AvoidNullReference`](./gridifyGlobalConfiguration.md#avoidnullreference) in the global configuration, but it allows you to enable this setting on a per-query basis instead of globally. When set to true, Gridify won't check for null values in collections during filtering operations.

- **Type:** `bool`
- **Default:** `false`

```csharp
var mapper = new GridifyMapper<Person>(q => q.AvoidNullReference = true);
```


### EntityFrameworkCompatibilityLayer

This setting is the same as [`EntityFrameworkCompatibilityLayer`](./extensions/entityframework.md#compatibility-layer) in the global configuration, but it allows you to enable this setting on a per-query basis instead of globally. When set to true, the EntityFramework Compatibility layer is enabled, making the generated expressions compatible with Entity Framework.

- **Type:** `bool`
- **Default:** `false`

```csharp
var mapper = new GridifyMapper<Person>(q => q.EntityFrameworkCompatibilityLayer = true);
```

## Filtering on Nested Collections

You can use LINQ `Select` and `SelectMany` methods to filter your data using its nested collections.

In this example, we have 3 nested collections, but filtering will apply to the `Property1` of the third level.

``` csharp
var mapper = new GridifyMapper<Level1>()
    .AddMap("prop1", l1 => l1.Level2List
            .SelectMany(l2 => l2.Level3List)
            .Select(l3 => l3.Property1));
// ...
var query = level1List.ApplyFiltering("prop1 = 123", mapper);
```

if you have only two-level nesting, you don't need to use `SelectMany`.

## Defining Mappings for Indexable Properties

Starting from version `v2.15.0`, GridifyMapper's `AddMap` method supports filtering on properties that are **indexable**, such as sub-collections, arrays, and dictionaries. This allows you to create dynamic queries by defining mappings to specific indexes or dictionary keys using square brackets `[ ]`.

### Mapping to Array Indexes

You can define a mapping to a specific index in an array or sub-collection by specifying the index within square brackets `[ ]`.`

```csharp
var gm = new GridifyMapper<TargetType>()
      .AddMap("arrayProp", (target, index) => target.MyArray[index].Prop);

var gq = new GridifyQuery
{
   // Filters on the 8th element of an array property
   Filter = "arrayProp[8] > 10"
};
```

### Mapping to Dictionary Keys

Similarly, you can define a mapping to a specific key in a dictionary or in a navigation property.

```csharp
var gm = new GridifyMapper<TargetType>()
      .AddMap("dictProp", (target, key) => target.MyDictionary[key]);

var gm2 = new GridifyMapper<TargetType>()
      .AddMap("navProp", (target, key) => target.NavigationProperty.Where(n => n.Key == key).Select(n => n.Value));

var gq = new GridifyQuery
{
   // Filters on the value associated with the 'name' key in a dictionary
   Filter = "dictProp[name] = John"
};
```

### Generic Overload for Non-String Dictionary Keys

If your dictionary key is not a string, you can use the generic overload of the `AddMap<T>` method to specify the key type.

```csharp
var gm = new GridifyMapper<TargetType>()
      .AddMap<Guid>("dictProp", (target, key) => target.MyDictionary[key]);
```

For more information on filtering using these mappings, refer to the [Using Indexers](./filtering.md#using-indexers).

## GetExpression

This method returns the selector expression that you can use it in LINQ queries.

``` csharp
Expression<Func<Person, object>> selector = mapper.GetExpression(nameof(Person.Name));
```

## GetLambdaExpression

This method returns the selector expression that you can use it in LINQ queries.

``` csharp
LambdaExpression selector = mapper.GetLambdaExpression(nameof(Person.Name));
```
