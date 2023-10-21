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

// Filter,Sort & Apply Paging
Paging<Person> result = personsRepo.Gridify(gq);
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

## GetFilteringExpression

This extension method, creates a lambda expression using the `GridifyQuery.Filter` property that you can use it in the LINQ `Where` method to filter the data.

``` csharp{2}
var gq = new GridifyQuery() { Filter = "name=John" };
Expression<Func<T, bool>> expression = gq.GetFilteringExpression<Person>();
var result = personsRepo.Where(expression);
```
