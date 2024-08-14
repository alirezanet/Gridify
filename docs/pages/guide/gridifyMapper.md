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

## HasMap

This method checks if the mapper has a mapping for the given field name.

## ClearMaps

This method clears the list of mappings

## GetCurrentMaps

This method returns list of current mappings.

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

### DefaultDateTimeKind

By setting this property to a `DateTimeKind` value, you can change the default `DateTimeKind` used when parsing dates.

- type: `DateTimeKind`
- default: `null`

``` csharp
var mapper = new GridifyMapper<Person>(q => q.DefaultDateTimeKind = DateTimeKind.Utc);
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
