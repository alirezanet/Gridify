# GridifyQuery

`GridifyQuery` is a simple class for configuring Filtering, Ordering, and Paging.

``` csharp
var gq = new GridifyQuery()
{
    Filter = "FirstName=John",
    Page = 1,
    PageSize = 20,
    OrderBy = "Age"
};

// Apply Filter, Sort and Paging
Paging<Person> result = personsRepo.Gridify(gq);
```

`GridifyQuery` also has an optional `Select` property for field projection — see [Selecting](#selecting) below.

## IsValid

This extension method checks if a `GridifyQuery` (`Filter`, `OrderBy`) is valid for use with a custom mapper or the auto-generated mapper. It returns `true` if valid, `false` otherwise.

The validation checks:
* Field names (mapped or actual properties)
* Filter syntax
* **Value type compatibility** (integers, `DateTime`, enums, `bool`, `Guid`, etc.)

### Basic usage

```csharp
var gq = new GridifyQuery { Filter = "name=John", OrderBy = "Age" };
// true
bool isValid = gq.IsValid<Person>();
```

```csharp
var gq = new GridifyQuery { Filter = "NonExist=John", OrderBy = "Age" };
// false (NonExist is not a property of Person)
bool isValid = gq.IsValid<Person>();
```

```csharp
var gq = new GridifyQuery { Filter = "@name=!", OrderBy = "Age" };
// false (invalid filter syntax)
bool isValid = gq.IsValid<Person>();
```

```csharp
var gq = new GridifyQuery { Filter = "Age=abc" };
// false (Age is an int, "abc" cannot be converted)
bool isValid = gq.IsValid<Person>();
```

### Using a custom mapper

Optionally you can pass a custom mapper to check if the `GridifyQuery` is valid for that mapper:

```csharp
var mapper = new GridifyMapper<Person>()
    .AddMap("name", q => q.Name);

var gq = new GridifyQuery { Filter = "name=John", OrderBy = "Age" };

// false (Age is not mapped on this mapper)
bool isValid = gq.IsValid(mapper);
```

### Getting validation error messages

If you need detailed feedback (for example, to return validation errors to a client), use the overload with `out List<string> validationErrors`:

```csharp
var gq = new GridifyQuery { Filter = "Age=abc" };

var isValid = gq.IsValid<Person>(out var errors);

// isValid == false
// errors might contain something like:
// ["Cannot convert value 'abc' to type 'Int32' for field 'Age': Invalid format"]
```

You can combine this with a custom mapper as well:

```csharp
var mapper = new GridifyMapper<Person>()
    .AddMap("name", q => q.Name);

var gq = new GridifyQuery { Filter = "name=John; Age=abc" };

var isValid = gq.IsValid(out var errors, mapper);

// isValid == false
// errors could include:
// - "Field 'Age' is not mapped" (if Age isn't mapped)
//   or, if it is mapped but the value is wrong:
// - "Cannot convert value 'abc' to type 'Int32' for field 'Age': Invalid format"
```

Notes:

* Empty or null `Filter` values are considered valid and return `true`.
* The “old” overloads (`IsValid<T>()` and `IsValid(mapper)`) remain and now also benefit from the improved value-type validation; they just don’t expose the error details.
* If `Select` is set, `IsValid` also validates each requested path against the mapper.

## Selecting

The `Select` property picks a subset of fields to project. Each projected item is a runtime-emitted type containing only the requested properties. Under Entity Framework Core this translates to column-pruned SQL.

``` csharp
var gq = new GridifyQuery()
{
    Filter = "age>18",
    OrderBy = "name",
    Page = 1,
    PageSize = 20,
    Select = "name,age,address.city"
};

Paging<object> result = dbContext.People.GridifySelect(gq);
```

`GridifySelect` is the projecting counterpart of `Gridify`: filter, order, page, and project in one call. If `Select` is null or empty, items remain boxed instances of the source type.

Dotted paths produce nested objects, and one level of collection projection is supported:

``` csharp
// nested object — result: { name, address: { city } }
"name,address.city"

// collection element — result: { name, orders: [ { amount } ] }
"name,orders.amount"
```

Two-level collection nesting (e.g. `orders.items.price`) throws `GridifySelectException`.

To validate a `Select` string in isolation, cast to `IGridifySelecting` and call `IsValidSelect`:

``` csharp
IGridifySelecting s = new GridifyQuery { Select = "name,doesNotExist" };
bool ok = s.IsValidSelect<Person>(out var errors);

// ok == false
// errors contains "Field 'doesNotExist' is not mapped"
```

By default, unmapped fields cause validation to fail and projection to throw. Set `IgnoreNotMappedFields = true` on the mapper to silently drop them.

::: warning
Field projection is not supported under **NativeAOT** (tracked at [#140](https://github.com/alirezanet/Gridify/issues/140)), and `Gridify.Elasticsearch` does not yet implement `Select`.
:::

## GetFilteringExpression

This extension method, creates a lambda expression using the `GridifyQuery.Filter` property that you can use it in the LINQ `Where` method to filter the data.

``` csharp{2}
var gq = new GridifyQuery() { Filter = "name=John" };
Expression<Func<T, bool>> expression = gq.GetFilteringExpression<Person>();
var result = personsRepo.Where(expression);
```
