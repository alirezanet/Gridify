# Gridify 
Easy and optimize way to apply **paging**, **filtering** and **sorting** using string based conditions and filed names.

The best use case of this library is Aspnet APIs. when you need to get some string base filtering conditions to filter data or sort them by a filed name or apply pagination concepts to you lists and return pageable, data grid ready information, from any repository or databases.


# Available Extentions
|      Extention | Description          
|----------------|-------------------------------|
|ApplyFiltering  | Apply Filtering using `string Filter` property of `QueryObject` class and returns an `IQueryable<T>`
|ApplyOrdering   | Apply Ordering using `string SortBy` and `bool IsSortAsc` properties of `QueryObject` class and returns an `IQueryable<T>`
|ApplyPaging     | Apply paging using `short Page` and `int PageSize` properties of `QueryObject` class and returns an `IQueryable<T>`
|ApplyOrderingAndPaging|Apply Both Ordering and paging and returns an `IQueryable<T>`
|ApplyEverything | Apply Filtering,Ordering and paging and returns an `IQueryable<T>`
|ApplyEverythingWithCount| Like ApplyEverything but it returns a tuple `(int Count,IQueryable<T> DataQuery)`. we can use `Count`, to create our pages.
|Gridify | Receives a `QueryObject` ,Load All requested data and returns `Paging<T>`. (Paging Class Has `int TotalItems` and `List<T> Items`)

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

# Basic Usage example

```c#
// usually, we don't need to create this object manually
// for example, we get this object as a parameter from our API Controller
var filter = new QueryObject() 
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


# Collaboration
Any collaboration to improve documentation and library is appreciated feel free to send pullrequest. <3





