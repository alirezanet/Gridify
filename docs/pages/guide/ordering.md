# Ordering

The ordering query expression can be built with a comma-delimited ordered list of field/property names followed by **`asc`** or **`desc`** keywords.

By default, if you don't add these keywords, Gridify assumes you need Ascending ordering.

:::: code-group

``` csharp [Extensions]
// asc - desc
var x = personsRepo.ApplyOrdering("Id"); // default ascending its equal to "Id asc"
var x = personsRepo.ApplyOrdering("Id desc"); // use descending ordering

// multiple orderings example
var x = personsRepo.ApplyOrdering("Id desc, FirstName asc, LastName");
```

``` csharp [GridifyQuery]
// asc - desc
var gq = new GridifyQuery() { OrderBy = "Id" }; // default ascending its equal to "Id asc"
var gq = new GridifyQuery() { OrderBy = "Id desc" }; // use descending ordering

// multiple orderings example
var gq = new GridifyQuery() { OrderBy = "Id desc, FirstName asc, LastName" };
```

``` csharp [QueryBuilder]
var builder = new QueryBuilder<Person>();
// asc - desc
builder.AddOrderBy("Id"); // default ascending its equal to "Id asc"
builder.AddOrderBy("Id desc"); // use descending ordering

// multiple orderings example
builder.AddOrderBy("Id desc, FirstName asc, LastName");
```

::::

## Order By Nullable types

Sometimes we need to order by nullable types, for example:

``` csharp
personsRepo.OrderBy(p => p.BirthDate.HasValue)
```

to support this behavior, you can use Gridify special characters (`?` or `!`) after the property name.

e.g:

To achieve the `personsRepo.OrderBy(p => p.BirthDate.HasValue)` query, you can use `?`:

``` csharp
var x = personsRepo.ApplyOrdering("BirthDate?");
```

and for `personsRepo.OrderBy(p => !p.BirthDate.HasValue)`, you can use `!`:

``` csharp
var x = personsRepo.ApplyOrdering("BirthDate!");
```

::: warning
These nullable characters will only work on the nullable types.
:::
