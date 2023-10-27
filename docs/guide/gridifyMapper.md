!!!include(./docs/guide/snippets/gridifyMapper.md)!!!

## Sub Collections

### Filtering on Nested Collections

You can use LINQ `Select` and `SelectMany` methods to filter your data using its nested collections.

In this example, we have 3 nested collections, but filtering will apply to the `Property1` of the third level.

``` csharp
var mapper = new GridifyMapper<Level1>()
    .AddMap("prop1", l1 => l1.Level2List
            .SelectMany(l2 => l2.Level3List)
            .Select(l3 => l3.Property1));
// ...
var query = level1List.ApplyFiltering("prop1 = 123", mapper);
```

if you have only two-level nesting, you don't need to use `SelectMany`.

### Use Indexes on Sub-Collections

Since version `v2.3.0`, GridifyMapper [AddMap](#addmap) method has a new overload that accepts a `index` parameter.
In the bellow example we want to filter data using `8th` index of our SubCollection.

``` csharp{4}
var gq = new GridifyQuery { Filter = "prop[8] > 10" };

var gm = new GridifyMapper<TargetType>()
      .AddMap("prop", (x , index) => x.SubCollection[index].SomeProp);
```

checkout [Passing Indexes](./filtering.md#passing-indexes) for more information.

## GetExpression

This method returns the selector expression that you can use it in LINQ queries.

``` csharp
Expression<Func<Person, object>> selector = mapper.GetExpression(nameof(Person.Name));
```

## GetLambdaExpression

This method returns the selector expression that you can use it in LINQ queries.

``` csharp
LambdaExpression selector = mapper.GetLambdaExpression(nameof(Person.Name));
```
