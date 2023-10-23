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
await client.SearchAsync<User>(s => s
    .Index("users")
    .ApplyFilteringOrderingPaging(gq));
```

## IsValid

This extension method, checks if the `GridifyQuery` (`Filter`, `OrderBy`) is valid to use with a custom mapper or the auto generated mapper and returns true or false.

``` csharp
var gq = new GridifyQuery() { Filter = "name=John" , OrderBy = "Age" };
// true
bool isValid = gq.IsValid<Person>();
```

``` csharp
var gq = new GridifyQuery() { Filter = "NonExist=John" , OrderBy = "Age" };
// false (NonExist is not a property of Person)
bool isValid = gq.IsValid<Person>();
```

``` csharp
var gq = new GridifyQuery() { Filter = "@name=!" , OrderBy = "Age" };
// false (this is not a valid filter)
bool isValid = gq.IsValid<Person>();
```

Optionally you can pass a custom mapper to check if the `GridifyQuery` is valid for that mapper.

``` csharp
var mapper = new GridifyMapper<Person>()
      .AddMap("name", q => q.Name);
var gq = new GridifyQuery() { Filter = "name=John" , OrderBy = "Age" };

// false (Age is not mapped)
bool isValid = gq.IsValid(mapper);
```
