# Extensions

::: warning
Some of the extensions are not provided by Gridify.Elasticsearch.
:::

The Gridify library adds below extension methods to `IQueryable` objects.

The Gridify.Elasticsearch library adds almost the same extensions methods as Gridify with some small differences.

All Gridify extension methods can accept [GridifyQuery](./gridifyQuery.md) and [GridifyMapper](./gridifyMapper.md) as parameter.
make sure to checkout the documentation of these classes for more information.

::: tip
If you want to use Gridify extension methods on an `IEnumerable` object, use `.AsQueryable()` first.
:::

## ApplyFiltering

You can use this method if you want to only apply **filtering** on a `IQueriable`, `DbSet` or `SearchRequestDescriptor`.

:::: code-group
::: code-group-item LINQ

``` csharp
var query = personsRepo.ApplyFiltering("name = John");
```

this is completely equivalent to the bellow LINQ query:

``` csharp
var query = personsRepo.Where(p => p.Name == "John");
```

the main difference is in the first example, we are using a string to filter, that can be dynamicly generated or passed from end-user but in the second example, we should hard code the query for supported fields.
:::

::: code-group-item Elasticsearch

``` csharp
await client.SearchAsync<User>(s => s
    .Index("users")
    .ApplyFiltering("name = John"));
```

this will make the next Elasticsearch query:

``` json
GET users/_search
{
  "query": {
    "term": {
      "name.keyword": {
        "value": "John"
      }
    }
  }
}
```

:::
::::

Checkout the [Filtering Operators](./filtering.md) section for more information.

## ApplyOrdering

You can use this method if you want to only apply **ordering** on an `IQueriable` collection, `DbSet`, or `SearchRequestDescriptor`.

:::: code-group
::: code-group-item LINQ

``` csharp
var query = personsRepo.ApplyOrdering("name, age desc");
```

this is completely equivalent to the bellow LINQ query:

``` csharp
var query = personsRepo.OrderBy(x => x.Name).ThenByDescending(x => x.Age);
```

:::

::: code-group-item Elasticsearch

``` csharp
await client.SearchAsync<User>(s => s
    .Index("users")
    .ApplyOrdering("name, age desc"));
```

this will make the next Elasticsearch query:

``` json
GET users/_search
{
  "sort": [
    {
      "name.keyword": {
        "order": "asc"
      }
    },
    {
      "age": {
        "order": "desc"
      }
    }
  ]
}
```

:::
::::

Checkout the [Ordering](./ordering.md) section for more information.

## ApplyPaging

You can use this method if you want to only apply **paging** on an `IQueryable` collection, `DbSet` or `SearchRequestDescriptor`.

:::: code-group
::: code-group-item LINQ

``` csharp
var query = personsRepo.ApplyPaging(3, 20);
```

this is completely equivalent to the bellow LINQ query:

``` csharp
var query = personsRepo.Skip((3-1) * 20).Take(20);
```

:::

::: code-group-item Elasticsearch

``` csharp
await client.SearchAsync<User>(s => s
    .Index("users")
    .ApplyPaging(3, 20));
```

this will make the next Elasticsearch query:

``` json
GET users/_search
{
  "from": 40,
  "size": 20
}
```

:::
::::

## ApplyFilteringAndOrdering

You can use this method if you want to apply **filtering** and **ordering** on an `IQueryable` collection, `DbSet` or `SearchRequestDescriptor`. This method accepts `IGridifyQuery`.

## ApplyOrderingAndPaging

You can use this method if you want to apply **ordering** and **paging** on an `IQueryable` collection, `DbSet` or `SearchRequestDescriptor`. This method accepts `IGridifyQuery`.

## ApplyFilteringOrderingPaging

You can use this method if you want to apply  **filtering** and **ordering** and **paging** on a `IQueryable` collection, `DbSet` or `SearchRequestDescriptor`. This method accepts `IGridifyQuery`.

## GridifyQueryable

::: warning
Is not supported by Gridify.Elasticsearch.
:::

Like [ApplyFilteringOrderingPaging](#ApplyFilteringOrderingPaging) but it returns a `QueryablePaging<T>` that have an extra `int Count` value that can be used for pagination.

## Gridify

::: warning
Is not supported by Gridify.Elasticsearch.
:::

This is an ALL-IN-ONE package, it accepts `IGridifyQuery`, applies filtering, ordering, and paging, and returns a `Paging<T>` object.
This method is completely optimized to be used with any **Grid** component.

## ToElasticsearchQuery

This extension can be useful if you need to apply additional filters to the query.

:::: code-group
::: code-group-item C#

``` csharp
var query = "name = John".ToElasticsearchQuery<User>();

await client.SearchAsync<User>(s => s
    .Index("users")
    .Query(query));
```

:::

::: code-group-item JSON

``` json
GET users/_search
{
  "query": {
    "term": {
      "name.keyword": {
        "value": "John"
      }
    }
  }
}
```

:::
::::

The ability to build `Query` object can be useful, if you need to apply additional filtering to it. E.g. you need to restrict data according to the organization.

:::: code-group
::: code-group-item C#

``` csharp
var query = "name = John".ToElasticsearchQuery<User>();
query &= new TermQuery("organizationId") { Value = 123 };

await client.SearchAsync<User>(s => s
    .Index("users")
    .Query(query));
```

:::

::: code-group-item JSON

``` json
GET users/_search
{
  "query": {
    "bool": {
      "must": [
        {
          "term": {
            "Name.keyword": {
              "value": "John"
            }
          }
        },
        {
          "term": {
            "organizationId": {
              "value": 123
            }
          }
        }
      ]
    }
  }
}
```

:::
::::

## ToElasticsearchSortOptions

This extension can be useful if you need to apply additional sorting to the query.

:::: code-group
::: code-group-item C#

``` csharp
var sort = "name, age desc".ToElasticsearchSortOptions<User>();

await client.SearchAsync<User>(s => s
    .Index("users")
    .Sort(sort));
```

:::

::: code-group-item JSON

``` json
GET users/_search
{
  "sort": [
    {
      "name.keyword": {
        "order": "asc"
      }
    },
    {
      "age": {
        "order": "desc"
      }
    }
  ]
}
```

:::
::::
