# GridifyQuery

`GridifyQuery` is a simple class for configuring Filtering, Ordering and Paging.

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

Here’s an updated version of the `IsValid` section you can drop into the docs.


## IsValid

This extension method checks if a `GridifyQuery` (`Filter`, `OrderBy`) is valid to use with a custom mapper or the auto-generated mapper and returns `true` or `false`.

* Field names (mapped or actual properties)
* Filter syntax
* **Value type compatibility** (ints, `DateTime`, enums, `bool`, `Guid`, etc.)

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

## GetFilteringExpression

This extension method, creates a lambda expression using the `GridifyQuery.Filter` property that you can use it in the LINQ `Where` method to filter the data.

``` csharp{2}
var gq = new GridifyQuery() { Filter = "name=John" };
Expression<Func<T, bool>> expression = gq.GetFilteringExpression<Person>();
var result = personsRepo.Where(expression);
```
