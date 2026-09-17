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

### Maps with a custom convertor

When a map has a convertor, `IsValid` runs it before checking the value type, the same way the
query builder does. Validating the raw text instead would reject values that only the convertor
understands:

```csharp
var mapper = new GridifyMapper<Person>()
    .AddMap("createdOn", q => q.CreatedOn, value => ParseRelativeDate(value)); // understands "d-2"

var gq = new GridifyQuery { Filter = "createdOn>d-2" };

// true, because the convertor turns "d-2" into a DateTime
bool isValid = gq.IsValid(mapper);
```

The convertor's result is then judged the way the query builder uses it:

| convertor result | validation |
| --- | --- |
| throws | invalid, the message includes the convertor's own error |
| a `string` | checked against the mapped property type, as usual |
| any other type | must be usable as the mapped property type |
| `null` | valid when the mapped property can hold null |

Two things to keep in mind:

* **`IsValid` invokes the convertor.** If yours is expensive or has side effects, it now runs
  during validation as well as during filtering.
* "Usable as the mapped property type" is decided by asking the query builder's own value
  machinery, so it follows whichever path is in effect. Plain LINQ needs the exact type;
  with the **Entity Framework compatibility layer** the value is assigned through reflection,
  which widens some types (`int` into a `long` property, for instance) and accepts `null` for
  a non-nullable property by storing its default.

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
