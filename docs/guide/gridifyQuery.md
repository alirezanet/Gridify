# GridifyQuery
GridifyQuery is a simple class for configuring Filtering, Ordering and Paging.

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

## GetFilteringExpression
This extension method, creates a lambda expression using the `GridifyQuery.Filter` property that you can use it in the LINQ `Where` method to filter the data.

``` csharp{2}
var gq = new GridifyQuery() { Filter = "name=John" };
Expression<Func<T, bool>> expression = gq.GetFilteringExpression<Person>();
var result = personsRepo.Where(expression);
```
