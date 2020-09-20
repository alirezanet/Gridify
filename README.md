# Gridify

<img alt="GitHub" src="https://img.shields.io/github/license/alirezanet/gridify"> <img alt="Nuget" src="https://img.shields.io/nuget/dt/gridify">

Easy and optimized way to apply **Filtering**, **Sorting** and **pagination** using text-based data.

The best use case of this library is Asp-net APIs. when you need to get some string base filtering conditions to filter data or sort them by a field name or apply pagination concepts to you lists and return a **pageable**, data grid ready information, from any repository or database.

---

## WebApi Usage example

```c#
// ApiController

[Produces(typeof(Paging<Person>))]
public IActionResult GetPersons(GridifyQuery filter)
{
    // Gridify => Returns Filtered and Sorted Pagination Data
    return myDbContext.Persons.Gridify(filter);
}

```

complete request sample:

```url
http://exampleDomain.com/api/GetPersons?pageSize=100&page=1&sortBy=FirstName&isSortAsc=false&filter=Age%3D%3D10
```

also we can totally ignore GridifyQuery

```url
http://exampleDomain.com/api/GetPersons
```

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

## Extentions
The library adds below extension methods to `IQueryable`:


| Extension              | Description                                                                                                                   |
| ---------------------- | ----------------------------------------------------------------------------------------------------------------------------- |
| ApplyFiltering         | Apply Filtering using `string Filter` property of `GridifyQuery` class and returns an `IQueryable<T>`                         |
| ApplyOrdering          | Apply Ordering using `string SortBy` and `bool IsSortAsc` properties of `GridifyQuery` class and returns an `IQueryable<T>`   |
| ApplyPaging            | Apply paging using `short Page` and `int PageSize` properties of `GridifyQuery` class and returns an `IQueryable<T>`          |
| ApplyOrderingAndPaging | Apply Both Ordering and paging and returns an `IQueryable<T>`                                                                 |
| ApplyEverything        | Apply Filtering,Ordering and paging and returns an `IQueryable<T>`                                                            |
| GridifyQueryable       | Like ApplyEverything but it returns a `QueryablePaging<T>` that have an extra `int totalItems` property to use for pagination |
| Gridify                | Receives a `GridifyQuery` ,Load All requested data and returns `Paging<T>`                                                    |

**TIP**:

`Gridify` function is an _ALL-IN-ONE package_, that applies **filtering** and **ordering** and **paging** to your data and returns a `Paging<T>`,

but for example, if you need to just filter your data without paging or sorting options you can use `ApplyFiltering` function instead.

---

## Supported Filtering Operators

| Name                  | Operator | Usage example                                             |
| --------------------- | -------- | --------------------------------------------------------- |
| Equal                 | `==`     | `"FieldName==Value"`                                      |
| NotEqual              | `!=`     | `"FieldName!=Value"`                                      |
| GreaterThan           | `<<`     | `"FieldName<<Value"`                                      |
| LessThan              | `>>`     | `"FieldName>>Value"`                                      |
| GreaterThanOrEqual    | `>=`     | `"FieldName>=Value"`                                      |
| LessThanOrEqual       | `<=`     | `"FieldName<=Value"`                                      |
| Contains - Like       | `=*`     | `"FieldName=*Value"`                                      |
| NotContains - NotLike | `!*`     | `"FieldName!*Value"`                                      |
| AND - &&              | `,`      | `"FirstName==Value,LastName==Value2"`                   |
| OR - &#124;&#124;     | <code>&#124;</code>  | <code>"FirstName==Value&#124;LastName==Value2"</code>
| Parenthesis           | `()`     | <code>"(FirstName=*Jo,Age<<30)&#124;(FirstName!=Hn,Age>>30)"</code> |

we can easily create complex queries using Parenthesis`()` with AND (`,`) + OR (`|`) operators.

---

## Basic Usage example

```c#
// usually, we don't need to create this object manually
// for example, we get this object as a parameter from our API Controller
var filter = new GridifyQuery()
{
    Filter = "FirstName==John",
    IsSortAsc = false,
    Page = 1,
    PageSize = 20,
    SortBy = "LastName"
};

Paging<Person> pData =
         myDbContext.Persons  // we can use Any list or repository or EntityFramework context
          .Gridify(filter); // Filter,Sort & Apply Paging


// pData.TotalItems => Count persons with 'John', First name
// pData.Items      => First 20 Persons with 'John', First Name
```

---

## Custom Mapping Support

By default Gridify is using a `GridifyMapper` object that automatically maps your string based field names to actual properties in your Entities but if you have a custom **DTO** (Data Transfer Object) you can create a custom instance of `GridifyMapper` and use it to create your mappings.

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
var filter = new GridifyQuery()
{
    Filter = "FirstName==John,Address=*st",
    IsSortAsc = true,
    SortBy = "PhoneNumber"
};

// myRepository: could be entity framework context or any other collections
var gridifiedData = myRepository.Persons.Gridify(filter, customMappings);


```

by default `GridifyMapper` is `Case-insensitive` but you can change this behavior if you need `Case-Sensitive` mappings.

```c#
var customMappings = new GridifyMapper<Person>(true); // mapper is case-sensitive now.
```

---

## Combine Gridify with AutoMapper

```c#
//AutoMapper ProjectTo + Filtering Only, example
var query = myDbContext.Persons.ApplyFiltering(gridifyQuery);
var result = query.ProjectTo<PersonDTO>().ToList();

// AutoMapper ProjectTo + Filtering + Ordering + Paging, example
QueryablePaging<Person> qp = myDbContext.Persons.GridifyQueryable(gridifyQuery);
var result = new Paging<Person> () { Items = qp.Query.ProjectTo<PersonDTO>().ToList (), TotalItems = qp.TotalItems };
```

## EntityFramework integration

if you need to use gridify **async** feature for entityFramework Core, use **`Gridify.EntityFramework`** package instead.

this package have two additional `GridifyAsync()` and `GridifyQueryableAsync()` functions.

```terminal
dotnet add package Gridify.EntityFramework
```

---

## Contribution

Any Contribution to improve documentation and library is appreciated feel free to send pull-Request. <3
