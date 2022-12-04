# Extensions
The Gridify library adds below extension methods to `IQueryable` objects.

All Gridify extension methods can accept [GridifyQuery](./gridifyQuery.md) and [GridifyMapper](./gridifyMapper.md) as parameter.
make sure to checkout the documentation of these classes for more information.

::: tip
If you want to use Gridify extension methods on an `IEnumerable` object, use `.AsQueryable()` first.
:::

## ApplyFiltering
You can use this method if you want to only apply **filtering** on a IQueriable or DbSet.

``` csharp
var query = personsRepo.ApplyFiltering("name = John");
```
this is completely equivalent to the bellow LINQ query:
``` csharp
var query = personsRepo.Where(p => p.Name == "John");
```

the main difference is in the first example, we are using a string to filter, that can be dynamicly generated or passed from end-user but in the second example, we should hard code the query for supported fields.

checkout the [Filtering Operators](./filtering.md) section for more information.

## ApplyOrdering
You can use this method if you want to only apply **ordering** on an `IQueriable` collection or `DbSet`.

``` csharp
var query = personsRepo.ApplyOrdering("name, age desc");
```
this is completely equivalent to the bellow LINQ query:
``` csharp
var query = personsRepo.OrderBy(x => x.Name).ThenByDescending(x => x.Age);
```
checkout the [Ordering](./ordering.md) section for more information.

## ApplyPaging
You can use this method if you want to only apply **paging** on an `IQueryable` collection or `DbSet`.

``` csharp
var query = personsRepo.ApplyPaging(3 , 20);
```
this is completely equivalent to the bellow LINQ query:
``` csharp
var query = personsRepo.Skip((3-1) * 20).Take(20);
```

## ApplyFilteringAndOrdering
You can use this method if you want to apply **filtering** and **ordering** on an `IQueryable` collection or `DbSet`. this method accepts `IGridifyQuery`.

## ApplyOrderingAndPaging
You can use this method if you want to apply **ordering** and **paging** on an `IQueryable` collection or `DbSet`. this method accepts `IGridifyQuery`.

## ApplyFilteringOrderingPaging
You can use this method if you want to apply  **filtering** and **ordering** and **paging** on a `IQueryable` collection or `DbSet`. this method accepts `IGridifyQuery`.

## GridifyQueryable
Like [ApplyFilteringOrderingPaging](#ApplyFilteringOrderingPaging) but it returns a `QueryablePaging<T>` that have an extra `int Count` value that can be used for pagination.

## Gridify
This is an ALL-IN-ONE package, it accepts `IGridifyQuery`, applies filtering, ordering, and paging, and returns a `Paging<T>` object.
this method is completely optimized to be used with any **Grid** component.


