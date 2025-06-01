# Gridify Client Library

The Gridify Client library is a lightweight JavaScript and TypeScript library designed to simplify the creation of
dynamic queries on the client side. This library facilitates the construction of queries that can be seamlessly
integrated with your server-side APIs, leveraging the powerful features of Gridify.

## Installation

To integrate the Gridify Client library into your project, install it using npm:

```shell-vue
npm i gridify-client
```

## GridifyQueryBuilder

The `GridifyQueryBuilder` interface represents the methods available for constructing dynamic queries using the Gridify
Client library.

When creating a new instance of `GridifyQueryBuilder`, you can pass an optional `GridifyQueryBuilderOptions` object to the constructor. This allows you to configure advanced behaviors:

- `from`: Clone an existing builder's state.
- `allowEmptyGroups`: Allow empty logical groups in the filter expression (default is `false`).

**Example:**

```ts
import { GridifyQueryBuilder } from "gridify-client";

const builder = new GridifyQueryBuilder({
  allowEmptyGroups: true
});
```

The following table describes the methods available in the GridifyQueryBuilder interface for constructing dynamic queries.

| Method       | Parameter                                          | Description                                                                                                           |
|--------------|----------------------------------------------------|-----------------------------------------------------------------------------------------------------------------------|
| setPage      | page: number                                       | Set the page number for pagination.                                                                                   |
| setPageSize  | pageSize: number                                   | Set the page size for pagination.                                                                                     |
| addOrderBy   | field: string, descending?: boolean                | Add ordering based on a field. If `descending` is `true`, the ordering is in descending order (default is ascending). |
| addCondition | field, operator, value, caseSensitive, escapeValue | Add filtering conditions. `caseSensitive` and `escapeValue` are optional parameters.                                  |
| startGroup   | -                                                  | Start a logical grouping of conditions.                                                                               |
| endGroup     | -                                                  | End the current logical group.                                                                                        |
| and          | optional?: boolean                                 | Add the logical AND operator. If `optional` is `true`, the operator will be ignored if it would be invalid in the current context. |
| or           | optional?: boolean                                 | Add the logical OR operator. If `optional` is `true`, the operator will be ignored if it would be invalid in the current context.  |
| build        | -                                                  | Build and retrieve the constructed query.                                                                             |

> **Note:**  
> The `and` and `or` methods accepts an optional boolean parameter. If set to `true`, the logical operator will be treated as optional and ignored if it would result in an invalid expression (such as being the first operator or consecutive with another operator).

## Conditional Operators

We can use the `ConditionalOperator` enum to access supported operators. we can pass these operators to the second parameter of `addCondition` method

| Operator             | Description                        |
|----------------------|------------------------------------|
| `Equal`              | Equality                           |
| `NotEqual`           | Inequality                         |
| `LessThan`           | Less Than                          |
| `GreaterThan`        | Greater Than                       |
| `GreaterThanOrEqual` | Greater Than or Equal              |
| `LessThanOrEqual`    | Less Than or Equal                 |
| `Contains`           | String Contains (LIKE)             |
| `NotContains`        | String Does Not Contain (NOT LIKE) |
| `StartsWith`         | String Starts With                 |
| `NotStartsWith`      | String Does Not Start With         |
| `EndsWith`           | String Ends With                   |
| `NotEndsWith`        | String Does Not End With           |

## Usage Example

Below is a basic example demonstrating the usage of the Gridify Client library in TypeScript. This example constructs a
dynamic query for pagination, ordering, and filtering:

``` ts
import { GridifyQueryBuilder, ConditionalOperator as op } from "gridify-client";

const query = new GridifyQueryBuilder()
  .setPage(2)
  .setPageSize(10)
  .addOrderBy("name", true)
  .startGroup()
    .addCondition("age", op.LessThan, 50)
    .or()
    .addCondition("name", op.StartsWith, "A")
  .endGroup()
  .and()
  .addCondition("isActive", op.Equal, true)
  .build();

console.log(query);

```

Output:

``` json
{
  "page": 2,
  "pageSize": 10,
  "orderBy": "name desc",
  "filter": "(age<50|name^A),isActive=true"
}
```

