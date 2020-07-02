# Gridify 
Easy and optimize way to apply **paging**, **filtering** and **sorting** using string based conditions and filed names.

The best use case of this library is Asp-net APIs. when you need to get some string base filtering conditions to filter data or sort them by a filed name or apply pagination concepts to you lists and return a **pageable**, data grid ready information, from any repository or databases.

installation using dotnet core CLI:
```
dotnet add package Gridify
```

---------------


# Available Extensions
|      Extension | Description          
|----------------|-------------------------------|
|ApplyFiltering  | Apply Filtering using `string Filter` property of `GridifyQuery` class and returns an `IQueryable<T>`
|ApplyOrdering   | Apply Ordering using `string SortBy` and `bool IsSortAsc` properties of `GridifyQuery` class and returns an `IQueryable<T>`
|ApplyPaging     | Apply paging using `short Page` and `int PageSize` properties of `GridifyQuery` class and returns an `IQueryable<T>`
|ApplyOrderingAndPaging|Apply Both Ordering and paging and returns an `IQueryable<T>`
|ApplyEverything | Apply Filtering,Ordering and paging and returns an `IQueryable<T>`
|ApplyEverythingWithCount| Like ApplyEverything but it returns a tuple `(int Count,IQueryable<T> DataQuery)`. we can use `Count`, to create our pages.
|Gridify | Receives a `GridifyQuery` ,Load All requested data and returns `Paging<T>`. (Paging Class Has `int TotalItems` and `List<T> Items`)

`Gridify` function is an *ALL-IN-ONE package*, that applies **filtering** and **ordering** and **paging** to your data and returns a `Paging<T>`,
but for example, if you need to just filter your data without paging or sorting options you can use `ApplyFiltering` function instead.


----------------


# Supported Filtering Operators 
| Name | Operator | Usage example
|------|-----------|-----|
| Equal | `==` | `"FieldName == Value"` |
| NotEqual | `!=` | `"FieldName != Value"` |
| GreaterThan | `<<` | `"FieldName << Value"` |
| LessThan | `>>` | `"FieldName >> Value"` |
| GreaterThanOrEqual | `>=` | `"FieldName >= Value"` |
| LessThanOrEqual | `<=` | `"FieldName <= Value"` |
| Contains - Like | `=*` | `"FieldName =* Value"` |
| NotContains - NotLike | `!*` | `"FieldName =* Value"` |
| AND - &&        | `,` | `"FirstName==Value , LastName==Value2"` |
| OR - \|\|       | `\|` | `"FirstName==Value \| LastName==Value2"` | 
| Parenthesis     | `()`| `"( FirstName=* Jo , Age<<30) \| ( FirstName != Hn , Age>>30 )"` |

we can easily create complex queries using Parenthesis`()` with AND (`,`) + OR (`|`) operators.


---------------


# Basic Usage example

```c#
// usually, we don't need to create this object manually
// for example, we get this object as a parameter from our API Controller
var filter = new GridifyQuery() 
{
    Filter = "FirstName == John",
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

------------------

# WebApi Usage example
```c#
// ApiController

[Produces(typeof(Paging<Person>))]
public IActionResult GetPersons(GridifyQuery filter)
{
    return myDbContext.Persons.Gridify(filter);
}

```
complete request sample:
```
http://exampleDomain.com/api/GetPersons?pageSize=100&page=1&sortBy=FirstName&isSortAsc=false&filter=Age%3D%3D10
```
also we can totally ignore GridifyQuery
```
http://exampleDomain.com/api/GetPersons
```

------------------


# Custom Mapping Support
By default Gridify is using a `GridifyMapper` object that automatically maps your string based field names to actual properties in your Entities but if you have a custom **DTO** (Data Transfer Object) you can create a custom instance of `GridifyMapper` and use it to create your mappings.

```c#
//// GridifyMapper Usage example -------------

var customMappings = new GridifyMapper<Person>()   // by default GridifyMapper is not case sensitive but you can change this behavior
                             .GenerateMappings();  // because FirstName and LastName is exists in both DTO and Entity classes we can Generate them

// add your custom mappings
customMappings.AddMap("address", q => q.Contact.Address )
              .AddMap("PhoneNumber", q => q.Contact.PhoneNumber );

// as i mentioned before. usually we don't need create this object manually because we can get this required data from an API or any Controller.
var filter = new GridifyQuery() 
{
    Filter = "FirstName == John , Address =* st",
    IsSortAsc = true,
    SortBy = "PhoneNumber"
};

// myRepository: could be entity framework context or any other collections 
var gridifiedData = myRepository.Gridify(filter, customMappings);

// DONE.

// ---------------------------
// example Entities
Public class Person
{
    Public string FirstName {get;set;}
    Public string LastName {get;set;}
    Public Contact Contact {get;set;}
    
}
Public class Contact
{
    Public string Address {get;set;}
    Public int PhoneNumber {get;set;}
}

// example DTO
public class PersonDTO
{
   Public string FirstName {get;set;}
   Public string LastName {get;set;}

   Public string Address {get;set;}
   Public int PhoneNumber {get;set;}
}

```

-----------------

# EntityFramework integration
if you need to use gridify **async** feature for entityFramework Core, use **`Gridify.EntityFramework`** package instead.
this package have two additional `GridifyAsync()` and `ApplyEverythingWithCountAsync()` functions.

```
dotnet add package Gridify.EntityFramework
```



# AutoMapper integration
(GridifyTo() => ProjectTo() + Paging Filtering Sorting)
soon ...

-----------------

# Collaboration
Any collaboration to improve documentation and library is appreciated feel free to send pull-Request. <3





