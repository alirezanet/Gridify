# Gridify

<img alt="GitHub" src="https://img.shields.io/github/license/alirezanet/gridify"> ![Nuget](https://img.shields.io/nuget/dt/gridify?color=%239100ff) ![Nuget](https://img.shields.io/nuget/v/gridify) ![GitHub branch checks state](https://img.shields.io/github/checks-status/alirezanet/gridify/master)

Easy and optimized way to apply **Filtering**, **Sorting** and **pagination** using text-based data.

The best use case of this library is Asp-net APIs. When you need to get some string base filtering conditions to filter data or sort it by a field name or apply pagination concepts to your lists and return a **pageable**, data grid ready information, from any repository or database.

**_You can find the version 1.x documentation on the [Version1 Branch](https://github.com/alirezanet/Gridify/tree/version-1.x)_**

---

## WebApi Simple Usage example

```c#
// ApiController

[Produces(typeof(Paging<Person>))]
public IActionResult GetPersons([FromQuery] GridifyQuery gQuery)
{
    // Gridify => Filter,Sort & Apply Paging 
    // in short, Gridify returns data especially for data Grids. 
    return myDbContext.Persons.Gridify(gQuery);
}

```

complete request sample:

```url
http://exampleDomain.com/api/GetPersons?pageSize=100&page=1&orderBy=FirstName&filter=Age%3D%3D10
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

### see more examples in the [tests](https://github.com/alirezanet/Gridify/blob/ca492ca943c3406c8a5f4c130097ee2e6d7e06ec/test/Core.Tests/GridifyExtensionsShould.cs#L22) 

---

## Performance comparison

Filtering is the most expensive feature in gridify. the following benchmark is comparing filtering in the most known dynamic linq libraries. As you can see, gridify has the closest result to the native linq.
Also, i Should note other features like Pagination and Sorting have almost zero overhead in Gridify.

BenchmarkDotNet=v0.13.0, OS=Windows 10.0.19043.1110 (21H1/May2021Update)
11th Gen Intel Core i5-11400F 2.60GHz, 1 CPU, 12 logical and 6 physical cores
.NET SDK=5.0.301
[Host]     : .NET 5.0.7 (5.0.721.25508), X64 RyuJIT


|      Method |       Mean |    Error |   StdDev | Ratio | RatioSD |   Gen 0 |  Gen 1 | Allocated |
|------------ |-----------:|---------:|---------:|------:|--------:|--------:|-------:|----------:|
| Native Linq |   869.3 us | 10.54 us |  9.86 us |  1.00 |    0.00 |  5.8594 | 2.9297 |     36 KB |
|     Gridify |   928.1 us | 13.41 us | 11.89 us |  1.07 |    0.02 |  6.8359 | 2.9297 |     46 KB |
|Dynamic Linq | 1,068.5 us | 10.66 us |  9.97 us |  1.23 |    0.02 | 19.5313 | 9.7656 |    122 KB |
|       Sieve | 1,126.8 us | 10.73 us | 10.04 us |  1.30 |    0.02 |  8.7891 | 3.9063 |     54 KB |

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

## Combine Gridify with AutoMapper

```c#
//AutoMapper ProjectTo + Filtering Only, example
var query = myDbContext.Persons.ApplyFiltering(gridifyQuery);
var result = query.ProjectTo<PersonDTO>().ToList();

// AutoMapper ProjectTo + Filtering + Ordering + Paging, example
QueryablePaging<Person> qp = myDbContext.Persons.GridifyQueryable(gridifyQuery);
var result = new Paging<Person> (qp.Count,qp.Query.ProjectTo<PersonDTO>().ToList ());
```

## EntityFramework integration

If you need to use the **async** feature for entityFramework core, use **`Gridify.EntityFramework`** package instead.

This package have two additional `GridifyAsync()` and `GridifyQueryableAsync()` functions.

```terminal
dotnet add package Gridify.EntityFramework
```

---

## Contribution

Any contribution to improve documentation and library is appreciated. Feel free to send pull-Requests. <3
