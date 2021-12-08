# GridifyMapper

Internally Gridify is using a auto generated mapper that maps your string base field names to actual properties in your entities, but sometimes we don't want to support filtering or sorting on a specific field. If you want to controll what field names are mapped to what properties, you can create a custom mapper.


to get a better understanding of how this works, consider the following example:

``` csharp
// sample Entities
public class Person
{
    public string UserName {get;set;}
    public string FirstName {get;set;}
    public string LastName {get;set;}
    public string Password {get;set;}
    public Contact Contact {get;set;}

}
public class Contact
{
    public string Address {get;set;}
    public int PhoneNumber {get;set;}
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
This method generates all the mappings for **top level public properties** of the entity.
we can use this method to generate the defult mappings.

## RemoveMap
This method removes mapping from the mapper. Usually you will use this method after you have generated the mappings to ignore some properties that you don't want to be supported by gridify filtering or ordering actions.

## AddMap
This method adds a mapping to the mapper.
- the first parameter is the name of the field you want to use in the string query.
- the second parameter is a propery selector expression.
- the third parameter is an optional [value convertor](#value-convertor) expression that you can use to convert user inputs to anything you want.

### Value convertor
If you need to change your search values before the filtering operation you can use this feature, the third parameter of the GridifyMapper AddMap method accepts a function that you can use to convert the input values.

in the above example we want to convert the userName value to lowercase before the filtering operation.
``` csharp
mapper = mapper.AddMap("userName", p => p.UserName, v => v.ToLower());
```

## HasMap
This method checks if the mapper has a mapping for the given field name.

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
var mapper = new GridifyMapper<Person>( q => q.CaseSensitive = true );
```

### IgnoreNotMappedFields
By setting this to `true` Gridify don't throw an exception when a field name is not mapped. for instance, in the above example, searching for `password` will not throw an exception.

- Type: `bool`
- Default: `false`

``` csharp
var mapper = new GridifyMapper<Person>( q => q.IgnoreNotMappedFields = true );
```


### AllowNullSearch
By setting this to `false`, Gridify don't allow searching on null values using the `null` keyword for values.

- Type: `bool`
- Default: `true`


``` csharp
var mapper = new GridifyMapper<Person>( q => q.AllowNullSearch = false );
```

## Sub Collections
### Filtering on Nested Collections
You can use LINQ `Select` and `SelectMany` methods to filter your data using its nested collections.

In this example, we have 3 nested collections, but filtering will apply to the `Property1` of the third level.
``` csharp
var mapper = new GridifyMapper<Level1>()
    .AddMap("prop1", l1 => l1.Level2List
            .SelectMany(l2 => l2.Level3List)
            .Select(l3 => l3.Property1);
// ...
var query = level1List.ApplyFiltering("prop1 = 123", mapper);
```

if you have only two-level nesting, you don't need to use `SelectMany`.

### Use Indexes on Sub-Collections

Since version `v2.3.0`, GridifyMapper [AddMap](#addmap) method had a new overload that accepts a `index` parameter.
In the bellow example we want to filter data using `8th` index of our SubCollection.

``` csharp{4}
var gq = new GridifyQuery { Filter = "prop[8] > 10" };

var gm = new GridifyMapper<TargetType>()
	      .AddMap("prop", (x , index) => x.SubCollection[index].SomeProp);
```

checkout [Passing Indexes](./filtering.md#passing-indexes) for more information.
