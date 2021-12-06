# AutoMapper

Gridify is complitely compatible with AutoMapper. also, these two packages can help each other nicely. we can use Gridify for filtering, sorting, and paging and AutoMapper for the projection.

``` csharp
// AutoMapper ProjectTo + Filtering Only, example
var query = personRepo.ApplyFiltering(gridifyQuery);
var result = query.ProjectTo<PersonDTO>().ToList();
```

``` csharp
// AutoMapper ProjectTo + Filtering + Ordering + Paging, example
QueryablePaging<Person> qp = personRepo.GridifyQueryable(gridifyQuery);
var result = new Paging<Person> (qp.Count, qp.Query.ProjectTo<PersonDTO>().ToList ());
```

## GridifyTo!
Gridify library does not have a built-in GridifyTo method because we don't want to have AutoMapper dependency. but if you are using AutoMapper in your project, I recommend you to add the bellow extension method to your project.

``` csharp
public static Paging<TDestination> GridifyTo<TSource, TDestination>(this IQueryable<TSource> query,
                    IMapper autoMapper, IGridifyQuery gridifyQuery, IGridifyMapper<TSource> mapper = null)
{
   mapper = mapper.FixMapper();
   var res = query.GridifyQueryable(gridifyQuery, mapper);
   return new Paging<TDestination> (res.Count , res.Query.ProjectTo<TDestination>(autoMapper.ConfigurationProvider).ToList());
}
```

``` csharp
// only if you have Gridify.EntityFramework package installed.
public static async Task<Paging<TDestination>> GridifyToAsync<TSource, TDestination>(this IQueryable<TSource> query,
                        IMapper autoMapper, IGridifyQuery gridifyQuery, IGridifyMapper<TSource> mapper = null)
{
   mapper = mapper.FixMapper();
   var res = await query.GridifyQueryableAsync(gridifyQuery, mapper);
   return new Paging<TDestination> (res.Count , await res.Query.ProjectTo<TDestination>(autoMapper.ConfigurationProvider).ToListAsync());
}
```
