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
Returns a lambda expression for filtering using the Filter property.

``` csharp{2}
var gq = new GridifyQuery() { Filter = "name=John" };
var expression = gq.GetFilteringExpression<Person>();
var result = personsRepo.Where(expression);
```
