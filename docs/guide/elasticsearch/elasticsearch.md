# Elasticsearch

## Gridify.Elasticsearch Package

The `Gridify.Elasticsearch` package has a bunch of [extension methods](./extensions.md) that allow to convert Gridify filters and sortings to Elasticsearch DSL queries using Elastic.Clients.Elasticsearch .NET client.

## CustomElasticsearchNamingAction

Specifies how field names are inferred from CLR property names. By default, **Elastic.Clients.Elasticsearch** uses camel-case property names.

- If `null` (default behavior) CLR property `EmailAddress` will be inferred as `emailAddress` Elasticsearch document field name.
- If, e.g., `p => p`, the CLR property `EmailAddress` will be inferred as `EmailAddress` Elasticsearch document field name.

:::: code-group
::: code-group-item Default

``` csharp
await client.SearchAsync<User>(s => s
    .Index("users")
    .ApplyFiltering("emailAddress = test@test.com"));
```

this will make the next Elasticsearch query:

``` json
GET users/_search
{
  "query": {
    "term": {
      "emailAddress.keyword": {
        "value": "test@test.com"
      }
    }
  }
}
```

:::

::: code-group-item Customized

``` csharp
GridifyGlobalConfiguration.CustomElasticsearchNamingAction = p => $"_{p}_";

await client.SearchAsync<User>(s => s
    .Index("users")
    .ApplyFiltering("emailAddress = John"));
```

this will make the next Elasticsearch query:

``` json
GET users/_search
{
  "query": {
    "term": {
      "_EmailAddress_.keyword": {
        "value": "test@test.com"
      }
    }
  }
}
```

:::

::::

## Examples of usage

### Without pre-initialized mapper

:::: code-group
::: code-group-item C#

``` csharp
var gq = new GridifyQuery()
{
    Filter = "FirstName=John",
    Page = 1,
    PageSize = 20,
    OrderBy = "Age"
};

var response = await client.SearchAsync<User>(s => s
    .Index("users")
    .ApplyPaging(gq)
    .ApplyFiltering(gp)
    .ApplyOrdering(gp));

return response.Documents;
```

:::

::: code-group-item JSON

``` json
GET users/_search
{
  "query": {
    "term": {
      "firstName.keyword": {
        "value": "John"
      }
    }
  },
  "from": 0,
  "size": 20,
  "sort": [{
    "age": {
      "order": "asc"
    }
  }]
}
```

:::
::::

### With custom mapping

:::: code-group
::: code-group-item C#

``` csharp
var gq = new GridifyQuery()
{
    Filter = "name=John, surname=Smith, age=30, totalOrderPrice=45",
    Page = 1,
    PageSize = 20,
    OrderBy = "Age"
};

var mapper = new GridifyMapper<User>()
     .AddMap("name", x => x.FirstName)
     .AddMap("surname", x => x.LastName)
     .AddMap("age", x => x.Age)
     .AddMap("totalOrderPrice", x => x.Order.TotalSum);

var response = await client.SearchAsync<User>(s => s
    .Index("users")
    .ApplyFilteringOrderingPaging(gq));

return response.Documents;
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
            "firstName.keyword": {
              "value": "John"
            }
          }
        },
        {
          "term": {
            "lastName.keyword": {
              "value": "Smith"
            }
          }
        },
        {
          "term": {
            "age": {
              "value": 30
            }
          }
        },
        {
          "term": {
            "order.totalSum": {
              "value": 45
            }
          }
        }
      ]
    }
  },
  "from": 0,
  "size": 20,
  "sort": [{
    "age": {
      "order": "asc"
    }
  }]
}
```

:::
::::

### With CustomElasticsearchNamingAction initialized

By default, Elasticsearch converts property names to camel-case for document fields. That's Gridify.Elasticsearch extensions work by default. But if it's necessary to apply a custom naming policy, it can also be customized.

:::: code-group
::: code-group-item C#

``` csharp
Func<string, string>? namingAction = p => $"_{p}_";
var mapper = new GridifyMapper<TestClass>(autoGenerateMappings: true)
{
   Configuration = { CustomElasticsearchNamingAction = namingAction }
};

var gq = new GridifyQuery()
{
    Filter = "FirstName=John",
    Page = 1,
    PageSize = 20,
    OrderBy = "Age"
};

var response = await client.SearchAsync<User>(s => s
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
      "_FirstName_.keyword": {
        "value": "John"
      }
    }
  },
  "from": 0,
  "size": 20,
  "sort": [{
    "_Age_": {
      "order": "asc"
    }
  }]
}
```

:::
::::
