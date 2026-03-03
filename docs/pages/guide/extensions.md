# Extensions

The Gridify library adds the following extension methods to `IQueryable` objects.

All Gridify extension methods can accept [GridifyQuery](./gridifyQuery.md) and [GridifyMapper](./gridifyMapper.md) as parameters.
Make sure to check out the documentation for these classes for more information.

::: tip
To use Gridify extension methods on an `IEnumerable` object, first call `.AsQueryable()` on it.
:::

## ApplyFiltering

Use this method to apply **filtering** to an `IQueryable` or `DbSet`.

``` csharp
var query = personsRepo.ApplyFiltering("name = John");
```

This is equivalent to the following LINQ query:

``` csharp
var query = personsRepo.Where(p => p.Name == "John");
```

With the `ApplyFiltering` method, you can use a raw string to filter data. This string can be generated dynamically or passed by the end-user (for example, through an API client or console input). In contrast, when using the LINQ `Where` method, you must hard-code the query for the supported fields.

Check out the [Filtering Operators](./filtering.md) section for more information.

## ApplyOrdering

Use this method to apply **ordering** to an `IQueryable` collection or `DbSet`.

``` csharp
var query = personsRepo.ApplyOrdering("name, age desc");
```

This is equivalent to the following LINQ query:

``` csharp
var query = personsRepo.OrderBy(x => x.Name).ThenByDescending(x => x.Age);
```

Check out the [Ordering](./ordering.md) section for more information.

## ApplyPaging

Use this method to apply **paging** to an `IQueryable` collection or `DbSet`.

``` csharp
var query = personsRepo.ApplyPaging(3, 20);
```

This is equivalent to the following LINQ query:

``` csharp
var query = personsRepo.Skip((3-1) * 20).Take(20);
```

## ApplyFilteringAndOrdering

Use this method to apply **filtering** and **ordering** to an `IQueryable` collection or `DbSet`. This method accepts `IGridifyQuery`.

## ApplyOrderingAndPaging

Use this method to apply **ordering** and **paging** to an `IQueryable` collection or `DbSet`. This method accepts `IGridifyQuery`.

## ApplyFilteringOrderingPaging

Use this method to apply **filtering**, **ordering**, and **paging** to an `IQueryable` collection or `DbSet`. This method accepts `IGridifyQuery`.

## GridifyQueryable

Like [ApplyFilteringOrderingPaging](#applyfilteringorderingpaging), but it returns a `QueryablePaging<T>` that includes an extra `int Count` value for pagination.

## Gridify

This is an all-in-one method. It accepts `IGridifyQuery`, applies filtering, ordering, and paging, and returns a `Paging<T>` object.
This method is optimized for use with any grid component.
