# Gridify (A Modern Dynamic LINQ library)

<img alt="GitHub" src="https://img.shields.io/github/license/alirezanet/gridify"> ![Nuget](https://img.shields.io/nuget/dt/gridify?color=%239100ff) ![Nuget](https://img.shields.io/nuget/v/gridify?label=stable) ![Nuget (with prereleases)](https://img.shields.io/nuget/vpre/gridify?label=latest) ![GitHub branch checks state](https://img.shields.io/github/checks-status/alirezanet/gridify/master?label=tests) <img alt="Zero-Dependencies" src="https://img.shields.io/badge/Dependencies-0-orange">

Easy and optimized way to apply **Filtering**, **Sorting** and **pagination** using text-based data.

Gridify is a **dynamic LINQ library** that converts your strings to a LINQ expression in the easiest way possible with really good performance.

The best use case of this library is Asp-net APIs. When you need to get some string base filtering conditions to filter data or sort it by a field name or apply pagination concepts to your lists and return a **pageable**, data grid ready information, from any repository or database.
Although, we are not limited to Asp.net projects and we can use this library on any .Net projects and on any collections.


**_You can find the version 1.x documentation on the [Version1 Branch](https://github.com/alirezanet/Gridify/tree/version-1.x)_**

---

## WebApi Simple Usage example

```c#
// ApiController

public Paging<Person> GetPersons([FromQuery] GridifyQuery gQuery)
{
    // Gridify => Filter,Sort & Apply Paging 
    // in short, Gridify returns data especially for data Grids. 
    return myDbContext.Persons.Gridify(gQuery);
}
```

complete request sample:

```url
http://exampleDomain.com/api/GetPersons?
          pageSize=100&
          page=1&
          orderBy=FirstName&
          filter=Age>10
```

Also, we can totally ignore GridifyQuery

```url
http://exampleDomain.com/api/GetPersons
```

---

## What is GridifyQuery (basic usage example)

GridifyQuery is a simple class for configuring Filtering,Paging,Sorting. 

```c#
// usually, we don't need to create this object manually
// for example, we get this object as a parameter from our API Controller
var gQuery = new GridifyQuery()
{
    Filter = "FirstName=John",
    Page = 1,
    PageSize = 20,
    OrderBy = "LastName"
};

Paging<Person> pData =
         myDbContext.Persons  // we can use Any list or repository or EntityFramework context
          .Gridify(gQuery); // Filter,Sort & Apply Paging


// pData.Count => Count persons with 'John', First name
// pData.Data      => First 20 Persons with 'John', First Name
```

## ApplyFiltering
Also, if you don't need paging and sorting features simply use `ApplyFiltering` extension instead of `Gridify`.

```c#
var query = myDbContext.Persons.ApplyFiltering("name = John");
// this is equal to : 
// myDbContext.Persons.Where(p => p.Name == "John");
```

### see more examples in the [tests](https://github.com/alirezanet/Gridify/blob/89ff1ce981726c20591b043fee72eb5faa68f458/test/Core.Tests/GridifyExtensionsShould.cs#L22) 

---

## Performance comparison

Filtering is the most expensive feature in gridify. the following benchmark is comparing filtering in the most known dynamic linq libraries. As you can see, gridify has the closest result to the native linq.
Also, i Should note other features like Pagination and Sorting have almost zero overhead in Gridify.

BenchmarkDotNet=v0.13.0, OS=Windows 10.0.19043.1165 (21H1/May2021Update)
11th Gen Intel Core i5-11400F 2.60GHz, 1 CPU, 12 logical and 6 physical cores
.NET SDK=5.0.301
[Host]     : .NET 5.0.7 (5.0.721.25508), X64 RyuJIT
DefaultJob : .NET 5.0.7 (5.0.721.25508), X64 RyuJIT


|      Method |       Mean |    Error |   StdDev | Ratio | RatioSD |   Gen 0 |   Gen 1 | Allocated |
|------------ |-----------:|---------:|---------:|------:|--------:|--------:|--------:|----------:|
| Native Linq |   737.0 us |  4.95 us |  4.39 us |  1.00 |    0.00 |  5.8594 |  2.9297 |     36 KB |
|     Gridify |   774.5 us | 13.60 us | 12.72 us |  1.05 |    0.02 |  6.8359 |  2.9297 |     46 KB |
| DynamicLinq |   902.7 us | 16.33 us | 15.28 us |  1.22 |    0.02 | 19.5313 |  9.7656 |    122 KB |
|       Sieve |   982.3 us | 11.06 us | 10.35 us |  1.33 |    0.01 |  8.7891 |  3.9063 |     54 KB |
|         Fop | 2,954.8 us | 14.16 us | 12.55 us |  4.01 |    0.03 | 50.7813 | 23.4375 |    311 KB |

---

## Installation

Install the [Gridify NuGet Package.](https://www.nuget.org/packages/Gridify/)

**Package Manager Console**
```
Install-Package Gridify
```
**.NET Core CLI**

```
dotnet add package Gridify
```
---

## Extensions
The library adds below extension methods to `IQueryable`:


| Extension              | Description                                                                                                                   |
| ---------------------- | ----------------------------------------------------------------------------------------------------------------------------- |
| ApplyFiltering (string)| Apply filtering using a raw `string` and returns an `IQueryable<T>`                         |
| ApplyFiltering (GridifyQuery)| Apply filtering using `string Filter` property of `GridifyQuery` class and returns an `IQueryable<T>`                         |
| ApplyOrdering          | Apply ordering using `string OrderBy` and `bool IsSortAsc` properties of `GridifyQuery` class and returns an `IQueryable<T>`   |
| ApplyPaging            | Apply paging using `short Page` and `int PageSize` properties of `GridifyQuery` class and returns an `IQueryable<T>`          |
| ApplyOrderingAndPaging | Apply both Ordering and paging and returns an `IQueryable<T>`                                                                 |
| ApplyFilterAndOrdering | Apply both filtering and ordering and returns an `IQueryable<T>`                                                                 |
| ApplyEverything        | Apply filtering,ordering and paging and returns an `IQueryable<T>`                                                            |
| GridifyQueryable       | Like ApplyEverything but it returns a `QueryablePaging<T>` that have an extra `int Count` property to use for pagination |
| Gridify                | Receives a `GridifyQuery` , loads All requested data and returns `Paging<T>`                                                    |

**TIP**:

`Gridify` function is an _ALL-IN-ONE package_, that applies **filtering** and **ordering** and **paging** to your data and returns a `Paging<T>`.

But for example, if you need to just filter your data without paging or sorting options you can use `ApplyFiltering` function instead.

---

## Supported Filtering Operators

| Name                  | Operator | Usage example                                             |
| --------------------- | -------- | --------------------------------------------------------- |
| Equal                 | `=`      | `"FieldName = Value"`                                      |
| NotEqual              | `!=`     | `"FieldName !=Value"`                                      |
| LessThan              | `<`      | `"FieldName < Value"`                                      |
| GreaterThan           | `>`      | `"FieldName > Value"`                                      |
| GreaterThanOrEqual    | `>=`     | `"FieldName >=Value"`                                      |
| LessThanOrEqual       | `<=`     | `"FieldName <=Value"`                                      |
| Contains - Like       | `=*`     | `"FieldName =*Value"`                                      |
| NotContains - NotLike | `!*`     | `"FieldName !*Value"`                                      |
| StartsWith            | `^`      | `"FieldName ^ Value"`                                      |
| NotStartsWith         | `!^`     | `"FieldName !^ Value"`                                     |
| EndsWith              | `$`      | `"FieldName $ Value"`                                      |
| NotEndsWith           | `!$`     | `"FieldName !$ Value"`                                     |
| AND - &&              | `,`      | `"FirstName = Value, LastName = Value2"`                   |
| OR - &#124;&#124;     | <code>&#124;</code>  | <code>"FirstName=Value&#124;LastName=Value2"</code>
| Parenthesis           | `()`     | <code>"(FirstName=*Jo,Age<30)&#124;(FirstName!=Hn,Age>30)"</code> |

We can easily create complex queries using parenthesis`()` with AND (`,`) + OR (`|`) operators.

**Escape character hint**:

Filtering has four special character `, | ( )` to handle complex queries. If you want to use these characters in your query values (after `=`), you should add a backslash <code>\ </code> before them.

JavaScript escape example:
```javascript
let esc = (v) => v.replace(/([(),|])/g, '\\$1')
```
Csharp escape example:
```csharp
var value = "(test,test2)";
var esc = Regex.Replace(value, "([(),|])", "\\$1" ); // esc = \(test\,test2\)
```

---

## Multiple OrderBy
OrderBy accepts comma-separated field names followed by `asc` or `desc` keyword.
by default, if you don't add these keywords,
gridify assumes you need Ascending ordering.

e.g
```c#
// asc - desc
var gq = new GridifyQuery() { OrderBy = "Id" }; // default assending its equal to "Id asc"
var gq = new GridifyQuery() { OrderBy = "Id desc" }; // use desending ordering

// multiple orderings example
var gq = new GridifyQuery() { OrderBy = "Id desc, FirstName asc, LastName" }; 
```

---

## Custom Mapping Support

By default Gridify is using a `GridifyMapper` object that automatically maps your string based field names to actual properties in your entities but if you have a custom **DTO** (Data Transfer Object) you can create a custom instance of `GridifyMapper` and use it to create your mappings.

```c#
// example Entities
public class Person
{
    public string FirstName {get;set;}
    public string LastName {get;set;}
    public Contact Contact {get;set;}

}
public class Contact
{
    public string Address {get;set;}
    public int PhoneNumber {get;set;}
}

// example DTO
public class PersonDTO
{
   public string FirstName {get;set;}
   public string LastName {get;set;}

   public string Address {get;set;}
   public int PhoneNumber {get;set;}
}

//// GridifyMapper Usage example -------------

var customMappings = new GridifyMapper<Person>()
        // because FirstName and LastName is exists in both DTO and Entity classes we can Generate them
        .GenerateMappings()
        // add custom mappings
        .AddMap("address", q => q.Contact.Address )
        .AddMap("PhoneNumber", q => q.Contact.PhoneNumber );


// as i mentioned before. usually we don't need create this object manually.
var gQuery = new GridifyQuery()
{
    Filter = "FirstName=John,Address=*st",
    OrderBy = "PhoneNumber"
};

// myRepository: could be entity framework context or any other collections
var gridifiedData = myRepository.Persons.Gridify(gQuery, customMappings);


```

By default `GridifyMapper` is `Case-insensitive` but you can change this behavior if you need `Case-Sensitive` mappings.

```c#
var customMappings = new GridifyMapper<Person>( q => 
{ 
   q.CaseSensitive = true;    // enalbe case-sensitvity.
   q.AllowNullSearch = false; // disable searching for null values
}); 
```
---

## Value Convertor
If you need to change your search values before the filtering operation you can use this feature, 
the third parameter of the GridifyMapper `AddMap` method accepts a function that you can use to convert the input values.
eg:
   
```c#
// convert values to lower case
var gm = new GridifyMapper<Person>()
       .AddMap("name" , q => q.FullName , v => v.ToLower() )
```

---

## Filtering on Nested Collections 
You can use LINQ `Select` and `SelectMany` methods to filter your data using its nested collections.

In this example, we have 3 nested collections, but filtering will apply to the `Property1` of the third level.
```c#
var gm = new GridifyMapper<Level1>()
    .AddMap("prop1", l1 => l1.Level2List.SelectMany(l2 => l2.Level3List).Select(l3 => l3.Property1);
 ```

if you have only two-level nesting, you don't need to use `SelectMany`. 

---
   
## EntityFramework integration

If you need to use the **async** feature for entityFramework core, use **`Gridify.EntityFramework`** package instead.

This package have two additional `GridifyAsync()` and `GridifyQueryableAsync()` functions.

```terminal
dotnet add package Gridify.EntityFramework
```
   
---
## Compile and Reuse
You can get Gridify generated expressions using the `GetFilteringExpression` and `GetOrderingExpression` methods, so you can store an expression and use it multiple times without having any overheads, also if you compile an expression you get a massive performance boost. but you should only use a compiled expression if you are not using Gridify alongside an ORM like Entity-Framework.
eg:
```c#
var gm = new GridifyMapper<Person>().GenerateMappings();
var gq = new GridifyQuery() {Filter = "name=John"};
var expression = gq.GetFilteringExpression(gm);
var compiledExpression = expression.Compile();  
```
   
**expression usage:**
```c#  
myDbContext.Persons.Where(expression);
```
   
**compiled expression usage:**
```c#
myPersonList.Where(compiledExpression);
```
   
   
This is the performance improvement example when you use a compiled expression
   
|          Method |         Mean |      Error |     StdDev | Ratio | RatioSD |    Gen 0 |   Gen 1 | Allocated |
|---------------- |-------------:|-----------:|-----------:|------:|--------:|---------:|--------:|----------:|
| GridifyCompiled |     1.837 us |  0.0201 us |  0.0179 us | 0.001 |    0.00 |   0.4692 |       - |     984 B |
|      NativeLinQ | 1,368.110 us | 19.5299 us | 17.3127 us | 1.000 |    0.00 |  17.5781 |  9.7656 |  37,348 B |
|         Gridify | 1,476.890 us | 14.2971 us | 11.9387 us | 1.079 |    0.02 |  21.4844 |  9.7656 |  48,116 B |

---
   
## Combine Gridify with AutoMapper

```c#
//AutoMapper ProjectTo + Filtering Only, example
var query = myDbContext.Persons.ApplyFiltering(gridifyQuery);
var result = query.ProjectTo<PersonDTO>().ToList();

// AutoMapper ProjectTo + Filtering + Ordering + Paging, example
QueryablePaging<Person> qp = myDbContext.Persons.GridifyQueryable(gridifyQuery);
var result = new Paging<Person> (qp.Count,qp.Query.ProjectTo<PersonDTO>().ToList ());
```
Or simply add these two extentions to your project
```c#
public static Paging<TDestination> GridifyTo<TSource, TDestination>(this IQueryable<TSource> query,
                        IMapper autoMapper, IGridifyQuery gridifyQuery, IGridifyMapper<TSource> mapper = null)
{
   mapper = mapper.FixMapper();
   var res = query.GridifyQueryable(gridifyQuery, mapper);
   return new Paging<TDestination> (res.Count , res.Query.ProjectTo<TDestination>(autoMapper.ConfigurationProvider).ToList());
}   

// only if you have Gridify.EntityFramework package installed.
public static async Task<Paging<TDestination>> GridifyToAsync<TSource, TDestination>(this IQueryable<TSource> query,
                        IMapper autoMapper, IGridifyQuery gridifyQuery, IGridifyMapper<TSource> mapper = null)
{
   mapper = mapper.FixMapper();
   var res = await query.GridifyQueryableAsync(gridifyQuery, mapper);
   return new Paging<TDestination> (res.Count , await res.Query.ProjectTo<TDestination>(autoMapper.ConfigurationProvider).ToListAsync());
}

```
   
---

## Contribution

Any contribution to improve documentation and library is appreciated. Feel free to send pull-Requests. <3
