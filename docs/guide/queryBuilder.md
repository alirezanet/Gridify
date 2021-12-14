# QueryBuilder

The QueryBuilder class is really useful if you want to manually build your query, also when you don't want to use the extension methods.

| Method                       | Description                                                                                                                   |
| ------------------------     | ----------------------------------------------------------------------------------------------------------------------------- |
| AddCondition                 | Adds a string base Filtering query    |
| AddOrderBy                   | Adds a string base Ordering query     |
| ConfigurePaging              | Configure Page and PageSize   |
| AddQuery                     | Accepts a GridifyQuery object to configure filtering,ordering and paging     |
| UseCustomMapper              | Accepts a GridifyMapper to use in build methods  |
| UseEmptyMapper               | Setup an Empty new GridifyMapper without auto generated mappings |
| AddMap                       | Add a single Map to existing mapper    |
| RemoveMap                    | Remove a single Map from existing mapper    |
| ConfigureDefaultMapper       | Configuring default mapper when we didn't use AddMapper method   |
| IsValid                      | Validates Condition, OrderBy, Query , Mapper and returns a bool  |
| Build                        | Applies filtering ordering and paging to a queryable context             |
| BuildCompiled                | Compiles the expressions and returns a delegate for applying filtering ordering and paging to a enumerable collection    |
| BuildFilteringExpression     | Returns filtering expression that can be compiled for later use for enumerable collections  |
| BuildEvaluator               | Returns an evaluator delegate that can be use to evaluate an queryable context    |
| BuildCompiledEvaluator       | Returns an compiled evaluator delegate that can be use to evaluate an enumerable collection   |
| BuildWithPaging              | Applies filtering ordering and paging to a context, and returns paging result     |
| BuildWithPagingCompiled      | Compiles the expressions and returns a delegate for applying filtering ordering and paging to a enumerable collection, that returns paging result    |
| BuildWithQueryablePaging     | Applies filtering ordering and paging to a context, and returns queryable paging result    |
| Evaluate                     | Directly Evaluate a context to check if all conditions are valid or not    |


``` csharp
var builder = new QueryBuilder<Person>()
        .AddCondition("name=John")
        .AddOrderBy("age, id");

 var query = builder.build(persons);
```
