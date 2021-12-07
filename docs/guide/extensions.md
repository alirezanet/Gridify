# Extensions
The Gridify library adds below extension methods to `IQueryable` and `IEnumerable` objects.

All Gridify extension methods can accept [GridifyQuery](/guide/gridifyQuery.html) and [GridifyMapper](/guide/gridifyMapper.html) as parameter.
make sure to check the documentation of these classes for more information.


## ApplyFiltering
You can use this method if you want to only apply **filtering** on a Quariable collection, DbSet or Enumerable list.

``` csharp
var query = personsRepo.ApplyFiltering("name = John");
```
this is completely equivalent to the bellow LINQ query:
``` csharp
var query = personsRepo.Where(p => p.Name == "John");
```

the main difference is in the first example, we are using a string to filter, that can be dynamicly generated or passed from end-user but in the second example, we should hard code the query for supported fields.

checkout the [Filtering Operators](/guide/filtering.html) section for more information.

## ApplyOrdering
You can use this method if you want to only apply **ordering** on a Quariable collection, DbSet or Enumerable list.

``` csharp
var query = personsRepo.ApplyOrdering("name, age desc");
```
this is completely equivalent to the bellow LINQ query:
``` csharp
var query = personsRepo.OrderBy(x => x.Name).ThenByDescending(x => x.Age);
```
checkout the [Ordering](/guide/ordering.html) section for more information.

## ApplyPaging
You can use this method if you want to only apply **paging** on a Quariable collection, DbSet or Enumerable list.

``` csharp
var query = personsRepo.ApplyPaging(3 , 20);
```
this is completely equivalent to the bellow LINQ query:
``` csharp
var query = personsRepo.Skip((3-1) * 20).Take(20);
```

## ApplyFilteringAndOrdering
You can use this method if you want to apply **filtering** and **ordering** on a Quariable collection or DbSet. this method accepts `IGridifyQuery`.

## ApplyOrderingAndPaging
You can use this method if you want to apply **ordering** and **paging** on a Quariable collection or DbSet. this method accepts `IGridifyQuery`.

## ApplyFilteringOrderingPaging
You can use this method if you want to apply  **filtering** and **ordering** and **paging** on a Quariable collection or DbSet. this method accepts `IGridifyQuery`.

## GridifyQueryable
Like [ApplyFilteringOrderingPaging](#ApplyFilteringOrderingPaging) but it returns a `QuaryablePaging<T>` that have an extra `int Count` value that can be used for pagination.

## Gridify
This is an ALL-IN-ONE package, it accepts `IGridifyQuery`, applys filtering, ordering and paging and returns a `Paging<T>` object.
this method is complitely optimized to be used with any **Grid** component.


