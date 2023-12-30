# Using Gridify in API Controllers

When working with ASP.NET APIs, especially when you need to apply string-based filtering conditions, sorting based on field names, or implementing pagination functionality, the Gridify library is a valuable tool. It can be used in any .NET project and with any type of collection, not just limited to ASP.NET projects.

To demonstrate the core concepts of Gridify, let's look at a simple implementation in the following example.

## Describing the Scenario

Imagine you have an API that returns a list of users. You want to use this API in your client-side application to display a list of users.

```csharp
// UserController
// ...
public IEnumerable<User> GetUsers()
{
    // context can be Entity Framework, a repository, or whatever.
    return context.Users.ToList();
}
```

*However, there are a few challenges:*
- The end-user may want to sort the list by name, age, or any other property.
- The end-user may want to filter the list by name, age, or any other property.
- Fetching the entire list of users is not efficient, so you need to add pagination.
- Returning a list of page size N is not enough. You also need to know the total number of users.

Implementing these features can be complex and messy. If you want to support multiple properties, you would need to write a lot of code with if-else statements. This is where Gridify comes in.

## Solving Problems Using Gridify

With Gridify, you can simplify your code and implement the required features in just a few lines:

```csharp
public Paging<User> GetUsers([FromQuery] GridifyQuery query)
{
    return context.Users.Gridify(query);
}
```

Gridify handles all the complexity behind the scenes.

## What is the Paging Return Value?

The `Paging` class is a generic Data Transfer Object (DTO) that has two properties:

```csharp
public int Count { get; set; }
public IEnumerable<T> Data { get; set; }
```

The `Count` property indicates the total number of records, while the `Data` property contains the records on the current page.

## What is GridifyQuery?

`GridifyQuery` is a class that represents the query parameters passed to the `Gridify` method.

[Learn more about GridifyQuery](../guide/gridifyQuery.md).

## Sample Request Query String

Please note that this URL is not encoded. Always remember to encode query strings before passing them to your APIs.

```
http://exampleDomain.com/api/GetUsers?
          pageSize=100&
          page=1&
          orderBy=FirstName&
          filter=Age>10
```

Alternatively, you can ignore the `GridifyQuery` and use the default pagination values, which are `pageSize=20` and `page=1`.

```
http://exampleDomain.com/api/GetUsers
```

## More Information

::: tip
- If you want to control which fields are supported for filtering or ordering, you can use the [GridifyMapper](../guide/gridifyMapper.md) class.
- All [Gridify extension methods](../guide/extensions.md) accept a `GridifyMapper` instance as a parameter.
- If you want to learn more about the [filtering](../guide/filtering.md) and [ordering](../guide/ordering.md) syntax, be sure to read the related documentation.
:::