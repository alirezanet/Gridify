# AutoMapper

Gridify is completely compatible with AutoMapper. Also, these two packages can help each other nicely. We can use Gridify for filtering, sorting, and paging and AutoMapper for the projection.

``` csharp
// AutoMapper ProjectTo + Filtering Only, example
var query = personRepo.ApplyFiltering(gridifyQuery);
var result = query.ProjectTo<PersonDTO>().ToList();
```

``` csharp
// AutoMapper ProjectTo + Filtering + Ordering + Paging, example
QueryablePaging<Person> qp = personRepo.GridifyQueryable(gridifyQuery);
var result = new Paging<Person>(qp.Count, qp.Query.ProjectTo<PersonDTO>().ToList());
```

## GridifyTo Extension

Filtering, ordering, paging, and projection are all done with a `GridifyTo` extension method.

The Gridify library does not include a built-in `GridifyTo` extension method because we don't want to add an AutoMapper dependency. However, if you are using AutoMapper in your project, we recommend adding the following extension method:

``` csharp
public static Paging<TDestination> GridifyTo<TSource, TDestination>(
   this IQueryable<TSource> query, IMapper autoMapper, IGridifyQuery gridifyQuery, IGridifyMapper<TSource> mapper = null)
{
   var res = query.GridifyQueryable(gridifyQuery, mapper);
   return new Paging<TDestination>(res.Count, res.Query.ProjectTo<TDestination>(autoMapper.ConfigurationProvider).ToList());
}
```

``` csharp
// only if you have Gridify.EntityFramework package installed.
public static async Task<Paging<TDestination>> GridifyToAsync<TSource, TDestination>(
   this IQueryable<TSource> query, IMapper autoMapper, IGridifyQuery gridifyQuery, IGridifyMapper<TSource> mapper = null)
{
   var res = await query.GridifyQueryableAsync(gridifyQuery, mapper);
   return new Paging<TDestination>(res.Count, await res.Query.ProjectTo<TDestination>(autoMapper.ConfigurationProvider).ToListAsync());
}
```
