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
| AddCompositeMap          | Adds a composite map that searches across multiple properties with OR logic                                                                        |
| AddNestedMapper          | Adds a nested mapper to reuse mappings from nested object types                                                                                    |
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

## AddNestedMapper

The `AddNestedMapper` method in `QueryBuilder<T>` allows you to reuse mapper configurations for nested objects when building queries dynamically. This mirrors the functionality available in `GridifyMapper<T>`, enabling DRY (Don't Repeat Yourself) mapper composition in query builders.

::: tip Related Documentation
For detailed information about nested mappers and their benefits, see the [AddNestedMapper documentation in GridifyMapper guide](./gridifyMapper.md#addnestedmapper).
:::

### Basic Usage Examples

#### Example 1: Reusing Address Mapper Without Prefix

```csharp
// Define a reusable address mapper
var addressMapper = new GridifyMapper<Address>()
    .AddMap("city", x => x.City)
    .AddMap("country", x => x.Country);

// Use in QueryBuilder - merges directly
var builder = new QueryBuilder<User>()
    .AddMap("email", x => x.Email)
    .AddNestedMapper(x => x.Address, addressMapper)
    .AddCondition("city=London")
    .AddOrderBy("email");

var result = builder.Build(users.AsQueryable());
// Supports: "city=London", "country=UK"
```

#### Example 2: Reusing Address Mapper With Prefix

```csharp
// Define a reusable address mapper
var addressMapper = new GridifyMapper<Address>()
    .AddMap("city", x => x.City)
    .AddMap("country", x => x.Country);

// Use in QueryBuilder - with custom prefix
var builder = new QueryBuilder<Company>()
    .AddMap("name", x => x.Name)
    .AddNestedMapper("location", x => x.Address, addressMapper)
    .AddCondition("location.city=Berlin")
    .ConfigurePaging(0, 10);

var result = builder.Build(companies.AsQueryable());
// Supports: "location.city=Berlin", "location.country=Germany"
```

#### Example 3: Custom Mapper Classes

```csharp
// Define a custom mapper class
public class AddressMapper : GridifyMapper<Address>
{
    public AddressMapper()
    {
        AddMap("city", q => q.City);
        AddMap("country", q => q.Country);
    }
}

// Use with QueryBuilder - without prefix
var builder = new QueryBuilder<User>()
    .AddMap("email", x => x.Email)
    .AddNestedMapper<AddressMapper>(x => x.Address);

// Use with QueryBuilder - with prefix
var builder = new QueryBuilder<Company>()
    .AddMap("name", x => x.Name)
    .AddNestedMapper<AddressMapper>("location", x => x.Address);
```

#### Example 4: Multiple Nested Mappers

```csharp
var addressMapper = new GridifyMapper<Address>()
    .AddMap("city", x => x.City)
    .AddMap("country", x => x.Country);

var contactMapper = new GridifyMapper<Contact>()
    .AddMap("phone", x => x.Phone)
    .AddMap("email", x => x.Email);

var builder = new QueryBuilder<User>()
    .AddMap("name", x => x.Name)
    .AddNestedMapper("addr", x => x.Address, addressMapper)
    .AddNestedMapper("contact", x => x.Contact, contactMapper)
    .AddCondition("addr.city=London,contact.phone=1234567890")
    .AddOrderBy("name");

var result = builder.Build(users.AsQueryable());
```

