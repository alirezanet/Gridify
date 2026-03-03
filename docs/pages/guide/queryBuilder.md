# QueryBuilder

The `QueryBuilder` class is useful when you want to manually build your query or when you prefer not to use the extension methods.

| Method                   | Description                                                                                                                                       |
|--------------------------|---------------------------------------------------------------------------------------------------------------------------------------------------|
| AddCondition             | Adds a string-based filtering query                                                                                                               |
| AddOrderBy               | Adds a string-based ordering query                                                                                                                |
| ConfigurePaging          | Configures Page and PageSize                                                                                                                      |
| AddQuery                 | Accepts a GridifyQuery object to configure filtering, ordering, and paging                                                                        |
| UseCustomMapper          | Accepts a GridifyMapper to use in build methods                                                                                                   |
| UseEmptyMapper           | Sets up an empty GridifyMapper without auto-generated mappings                                                                                    |
| AddMap                   | Adds a single map to the existing mapper                                                                                                          |
| RemoveMap                | Removes a single map from the existing mapper                                                                                                     |
| ConfigureDefaultMapper   | Configures the default mapper when UseCustomMapper method is not used                                                                             |
| IsValid                  | Validates Condition, OrderBy, Query, and Mapper; returns a boolean                                                                                |
| Build                    | Applies filtering, ordering, and paging to a queryable context                                                                                    |
| BuildCompiled            | Compiles the expressions and returns a delegate for applying filtering, ordering, and paging to an enumerable collection                          |
| BuildFilteringExpression | Returns a filtering expression that can be compiled for later use with enumerable collections                                                     |
| BuildEvaluator           | Returns an evaluator delegate that can be used to evaluate a queryable context                                                                    |
| BuildCompiledEvaluator   | Returns a compiled evaluator delegate that can be used to evaluate an enumerable collection                                                       |
| BuildWithPaging          | Applies filtering, ordering, and paging to a context and returns a paging result                                                                  |
| BuildWithPagingCompiled  | Compiles the expressions and returns a delegate for applying filtering, ordering, and paging to an enumerable collection that returns paging result |
| BuildWithQueryablePaging | Applies filtering, ordering, and paging to a context and returns a queryable paging result                                                        |
| Evaluate                 | Directly evaluates a context to check if all conditions are valid                                                                                 |

``` csharp
var builder = new QueryBuilder<Person>()
        .AddCondition("name=John")
        .AddOrderBy("age, id");

var query = builder.Build(persons);
```
