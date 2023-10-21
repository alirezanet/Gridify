# Filtering

::: warning
Not all features described here are supported by Gridify.Elasticsearch.
:::

Gridify supports the following filtering operators:

## Conditional Operators

| Name                  | Operator | Usage example          |
|-----------------------|----------|------------------------|
| Equal                 | `=`      | `"FieldName = Value"`  |
| NotEqual              | `!=`     | `"FieldName !=Value"`  |
| LessThan              | `<`      | `"FieldName < Value"`  |
| GreaterThan           | `>`      | `"FieldName > Value"`  |
| GreaterThanOrEqual    | `>=`     | `"FieldName >=Value"`  |
| LessThanOrEqual       | `<=`     | `"FieldName <=Value"`  |
| Contains - Like       | `=*`     | `"FieldName =*Value"`  |
| NotContains - NotLike | `!*`     | `"FieldName !*Value"`  |
| StartsWith            | `^`      | `"FieldName ^ Value"`  |
| NotStartsWith         | `!^`     | `"FieldName !^ Value"` |
| EndsWith              | `$`      | `"FieldName $ Value"`  |
| NotEndsWith           | `!$`     | `"FieldName !$ Value"` |

::: tip
If you don't specify any value after `=` or `!=` operators, Gridify search for the `default` and `null` values.

``` csharp
var x = personsRepo.ApplyFiltering("name=");
```

this is equivalent to the bellow LINQ query:

``` csharp
var x = personsRepo.Where(p =>
             p.Name is null ||
             p.Name == string.Empty );
```

:::

::: warning
For now, it works a bit differently in Gridify.Elasticsearch compared to Gridify. It will be fixed in the future to be
consistent.
:::

## Special Operators

### Logical Operators

Using logical operators we easily can create complex queries.

| Name        | Operator            | Usage example                                                     |
|-------------|---------------------|-------------------------------------------------------------------|
| AND         | `,`                 | `"FirstName = Value, LastName = Value2"`                          |
| OR          | <code>&#124;</code> | <code>"FirstName=Value&#124;LastName=Value2"</code>               |
| Parenthesis | `()`                | <code>"(FirstName=*Jo,Age<30)&#124;(FirstName!=Hn,Age>30)"</code> |

### Case Insensitive Operator - /i

::: warning
Is not supported by Gridify.Elasticsearch.
:::

The **'/i'** operator can be use after string values for case insensitive searches.
You should only use this operator after the search value.

Example:

``` csharp
var x = personsRepo.ApplyFiltering("FirstName=John/i");
```

this query matches with JOHN - john - John - jOHn ...

## Escaping

Gridify have five special operators  `, | ( ) /i` to handle complex queries and case-insensitive searches. If you want
to use these characters in your query values (after conditional operator), you should add a backslash <code>\ </code>
before them. having this regex could be helpful `([(),|]|\/i)`.

JavaScript escape example:

``` javascript
let esc = (v) => v.replace(/([(),|]|\/i)/g, '\\$1')
```

Csharp escape example:

``` csharp
var value = "(test,test2)";
var esc = Regex.Replace(value, "([(),|]|\/i)", "\\$1" );
// esc = \(test\,test2\)
```

## Passing Indexes

::: warning
Is not supported by Gridify.Elasticsearch.
:::

Since version `v2.3.0`, Gridify support passing indexes to the sub collections. We can pass the index using the `[ ]`
brackets.
In the bellow example we want to filter data using `8th` index of our SubCollection.

``` csharp{6}
var gm = new GridifyMapper<TargetType>()
      .AddMap("prop", (x , index) => x.SubCollection[index].SomeProp);

var gq = new GridifyQuery
{
    Filter = "prop[8] > 10"
};
```

Checkout [Use Indexes on Sub-Collections](./gridifyMapper.md#use-indexes-on-sub-collections) for more information.

## Custom Operators

::: warning
Is not supported by Gridify.Elasticsearch.
:::

Sometimes the default Gridify operators are not enough, For example, if you need an operator for regex matching or when
you are using the EntityFramework, you may want to use `EF.Functions.FreeText` rather than a LIKE with wildcards. In
this case, you can define your own operators. (added in `v2.6.0`)

To define a custom operator, you need to create a class that implements the `IGridifyOperator` interface. then you need
to register it through the global [CustomOperators](./gridifyGlobalConfiguration.md#customoperators) configuration.

::: tip
Custom operators must be start with the `#` character.
:::

- FreeTextOperator Example:

```csharp
class FreeTextOperator : IGridifyOperator
{
   public string GetOperator() => "#=";
   public Expression<OperatorParameter> OperatorHandler()
   {
      return (prop, value) => EF.Functions.FreeText(prop, value.ToString());
   }
}
```

- RegexMatchOperator Example:

```csharp
class RegexMatchOperator : IGridifyOperator
{
   public string GetOperator() => "#%";
   public Expression<OperatorParameter> OperatorHandler()
   {
      return (prop, value) => Regex.IsMatch(prop.ToString(), value.ToString());
   }
}
```

Registration Example:

```csharp
 GridifyGlobalConfiguration.CustomOperators.Register(new FreeTextOperator());
 GridifyGlobalConfiguration.CustomOperators.Register(new RegexMatchOperator());
```
