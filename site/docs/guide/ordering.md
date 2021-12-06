# Ordering Syntax

The ordering query expression can be built with a comma-separated field names followed by **`asc`** or **`desc`** keywords.

by default, if you don't add these keywords, gridify assumes you need Ascending ordering.

:::: code-group
::: code-group-item Extentions
``` csharp
// asc - desc
var x = PersonsList.ApplyOrdering("Id"); // default assending its equal to "Id asc"
var x = PersonsList.ApplyOrdering("Id desc"); // use desending ordering

// multiple orderings example
var x = PersonsList.ApplyOrdering("Id desc, FirstName asc, LastName");
```
:::

::: code-group-item GridifyQuery
``` csharp
// asc - desc
var gq = new GridifyQuery() { OrderBy = "Id" }; // default assending its equal to "Id asc"
var gq = new GridifyQuery() { OrderBy = "Id desc" }; // use desending ordering

// multiple orderings example
var gq = new GridifyQuery() { OrderBy = "Id desc, FirstName asc, LastName" };
```
:::

::: code-group-item QueryBuilder
``` csharp
var builder = new QueryBuilder<Person>();
// asc - desc
builder.AddOrderBy("Id"); // default assending its equal to "Id asc"
builder.AddOrderBy("Id desc"); // use desending ordering

// multiple orderings example
builder.AddOrderBy("Id desc, FirstName asc, LastName");
```
:::
::::
