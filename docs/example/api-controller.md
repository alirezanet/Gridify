---
sidebar: 'auto'
---

## Using Gridify in API Controllers

One of the best use cases of this library is Asp-net APIs, When you need to get some string base filtering conditions to filter data or sort it by a field name or apply pagination concepts to your lists and return a pageable, data grid ready information, from any repository or database. Although, we are not limited to Asp.net projects and we can use this library on any .Net projects and on any collections.

In following, we will see a simple example but it is enough to understand the basic concepts of Gridify.

### Descibing Scenario

Imagine you have an API that returns a list of users. We want to use this API in our client side application to show a list of users.

``` csharp
// UserController
// ...
public IEnumerable<User> GetUsers()
{
    // context can be entity framework, a repository or whatever
    return context.Users.ToList();
}

```
*There are a few problems:*
- The end-user may want to **sort** the list by name, or by age, or by any other property.
- The end-user may want to **filter** the list by name, or by age, or by any other property.
- Fetching entire list of users is not efficient, so we need somehow add pagination.
- Returning a list of page size `N`is not enough, we also need to know the total number of users.

Implementing these features is not easy or at least **clean**. we need to write a lot of code with if-else statements if we want to support all reasonable properties.
This is where Gridify comes in.

### Solving problems using Gridify

``` csharp
public Paging<User> GetUsers([FromQuery] GridifyQuery query)
{
    return context.Users.Gridify(query);
}
```
All the magic is done by Gridify.


### What is Paging return value?
The Paging class is simply a generic DTO(Data Transfer Object) That has two properties:
``` csharp
public int Count { get; set; }
public IEnumerable<T> Data { get; set; }
```


### What is GridifyQuery?
GridifyQuery is a class that represents the query parameters that are passed to the Gridify method.

[Learn more about GridifyQuery](../guide/gridifyQuery.md).


### Sample request query string
to make the example readable this **isn't encoded** url,
make sure to always **encode** the query strings before passing it to your APIs
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
- If you want to controll what fields should be supported for filtering or ordering, you can use the [GridifyMapper](../guide/gridifyMapper.md) class.

- All [gridify extension methods](../guide/extensions.md) accept a GridifyMapper instance as a parameter.

- If you want to lean more about [filtering](../guide/filtering.md) and [ordering](../guide/ordering.md) syntex, make sure to read related documentations.
:::
