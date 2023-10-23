# Extensions

The Gridify.Elasticsearch library adds almost the same extension methods as Gridify with some small differences.

Most Gridify extension methods can accept [GridifyQuery](./gridifyQuery.md) and [GridifyMapper](./gridifyMapper.md) as parameter.
Make sure to checkout the documentation of these classes for more information.

## ApplyFiltering

You can use this method if you want to only apply **filtering** on the `SearchRequestDescriptor` descriptor.

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

Checkout the [Filtering Operators](./filtering.md) section for more information.

## ApplyOrdering

You can use this method if you want to only apply **ordering** on the `SearchRequestDescriptor` descriptor.

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

Checkout the [Ordering](./ordering.md) section for more information.

## ApplyPaging

You can use this method if you want to only apply **paging** on an the `SearchRequestDescriptor` descriptor.

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

## ApplyFilteringAndOrdering

You can use this method if you want to apply **filtering** and **ordering** on the `SearchRequestDescriptor` descriptor. This method accepts `IGridifyQuery`.

:::: code-group
::: code-group-item C#

``` csharp
var gq = new GridifyQuery { Filter = "name = John", OrderBy = "name, age desc" };

await client.SearchAsync<User>(s => s
    .Index("users")
    .ApplyFilteringAndOrdering(gq));
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
  },
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

## ApplyOrderingAndPaging

:::: code-group
::: code-group-item C#

``` csharp
var gq = new GridifyQuery { OrderBy = "name, age desc", Page = 3, PageSize = 20 };

await client.SearchAsync<User>(s => s
    .Index("users")
    .ApplyOrderingAndPaging(gq));
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
  ],
  "from": 40,
  "size": 20
}
```

:::
::::

You can use this method if you want to apply **ordering** and **paging** on the `SearchRequestDescriptor` descriptor. This method accepts `IGridifyQuery`.

## ApplyFilteringOrderingPaging

You can use this method if you want to apply  **filtering** and **ordering** and **paging** on the `SearchRequestDescriptor` descriptor. This method accepts `IGridifyQuery`.

:::: code-group
::: code-group-item C#

``` csharp
var gq = new GridifyQuery
{
   Filter = "name = John",
   OrderBy = "name, age desc",
   Page = 3,
   PageSize = 20
};

await client.SearchAsync<User>(s => s
    .Index("users")
    .ApplyFilteringOrderingPaging(gq));
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
  },
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
  ],
  "from": 40,
  "size": 20
}
```

:::
::::

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
