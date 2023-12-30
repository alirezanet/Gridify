# Compile and Reuse

You can access Gridify generated expressions using the `GetFilteringExpression` of [GridifyQuery](./gridifyQuery.md)
or `BuildCompiled` methods of [QueryBuilder](./queryBuilder.md) class,
by storing an expression you can use it multiple times without having any overheads,
also if you store a compiled expression you get a massive performance boost.

::: warning
You should only use a **compiled** expression (delegate) if you are **not** using Gridify alongside an ORM like
Entity-Framework.
:::

``` csharp
// eg.1 - using GridifyQuery - Compiled - where only
var gq = new GridifyQuery() { Filter = "name=John" };
var expression = gq.GetFilteringExpression<Person>();
var compiledExpression = expression.Compile();
var result = persons.Where(compiledExpression);
```

``` csharp
// eg.2 - using QueryBuilder - Compiled - where only
var compiledExpression = new QueryBuilder<Person>()
                         .AddCondition("name=John")
                         .BuildFilteringExpression()
                         .Compile();
var result = persons.Where(compiledExpression);
```

``` csharp
// eg.3 - using QueryBuilder - BuildCompiled
var func = new QueryBuilder<Person>()
          .AddCondition("name=John")
          .BuildCompiled();
var result = func(persons);

```

## Performance

This is the performance improvement example when you use a compiled expression.

| Method          |       Mean | Ratio | RatioSD |  Gen 0 |  Gen 1 | Allocated |
|-----------------|-----------:|------:|--------:|-------:|-------:|----------:|
| GridifyCompiled |   1.008 us | 0.001 |    0.00 | 0.1564 |      - |     984 B |
| Gridify         | 689.329 us | 1.000 |    0.00 | 5.8594 | 2.9297 |  39,924 B |
| NativeLINQ      | 736.854 us | 1.019 |    0.01 | 5.8594 | 2.9297 |  37,392 B |
