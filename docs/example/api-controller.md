---
sidebar: 'auto'
---

## Using Gridify in API Controllers

One of the best use cases for this library is in ASP.NET APIs when you need to use string-based filtering conditions to filter data, sort it by a field name, or apply pagination concepts to your lists and return pageable, data grid ready information from any repository or database. However, we are not limited to ASP.NET projects, and this library can be used in any .NET project and on any type of collection.

In the following example, we'll provide a simple implementation that will help us understand the basic concepts of Gridify.

### Descibing Scenario

Let's imagine you have an API that returns a list of users. You want to use this API in your client-side application to show a list of users.

``` csharp
// UserController
// ...
public IEnumerable<User> GetUsers()
{
    // context can be Entity Framework, a repository, or whatever.
    return context.Users.ToList();
}

```
*However, there are a few problems:*
- The end-user may want to sort the list by name, age, or any other property.
- The end-user may want to filter the list by name, age, or any other property.
- Fetching the entire list of users is not efficient, so you need to add pagination.
- Returning a list of page size N is not enough. You also need to know the total number of users.

Implementing these features is not easy, or at least, not clean. If you want to support all reasonable properties, you need to write a lot of code with if-else statements. This is where Gridify comes in.

### Solving problems using Gridify
With Gridify, you can simplify your code and implement the required features in a few lines:

``` csharp
public Paging<User> GetUsers([FromQuery] GridifyQuery query)
{
    return context.Users.Gridify(query);
}
```
Gridify handles all the complexity behind the scenes.


### What is Paging return value?
The Paging class is simply a generic Data Transfer Object (DTO) that has two properties:
``` csharp
public int Count { get; set; }
public IEnumerable<T> Data { get; set; }
```
`Count` indicates the total number of records, while `Data` contains the records on the current page.

### What is GridifyQuery?
`GridifyQuery` is a class that represents the query parameters passed to the `Gridify` method.

[Learn more about GridifyQuery](../guide/gridifyQuery.md).


### Sample request query string
Please note that this URL is not encoded. Always remember to encode query strings before passing them to your APIs.
```
http://exampleDomain.com/api/GetUsers?
          pageSize=100&
          page=1&
          orderBy=FirstName&
          filter=Age>10
```
Also, we can totally ignore GridifyQuery, and just use pagination default values which is `pageSize=20` and `page=1`
```
http://exampleDomain.com/api/GetUsers
```

### More Information

::: tip
- If you want to control which fields are supported for filtering or ordering, you can use the [GridifyMapper](../guide/gridifyMapper.md) class.

- All [Gridify extension methods](../guide/extensions.md) accept a `GridifyMapper` instance as a parameter.

- If you want to learn more about the [filtering](../guide/filtering.md) and [ordering](../guide/ordering.md) syntax, be sure to read the related documentation.
:::
