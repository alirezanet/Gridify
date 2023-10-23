# Filtering

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

## Special Operators

### Logical Operators

Using logical operators we easily can create complex queries.

| Name        | Operator            | Usage example                                                     |
|-------------|---------------------|-------------------------------------------------------------------|
| AND         | `,`                 | `"FirstName = Value, LastName = Value2"`                          |
| OR          | <code>&#124;</code> | <code>"FirstName=Value&#124;LastName=Value2"</code>               |
| Parenthesis | `()`                | <code>"(FirstName=*Jo,Age<30)&#124;(FirstName!=Hn,Age>30)"</code> |

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
