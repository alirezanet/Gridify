# EzPaging
Easy and optimized way to apply paging - filtering and sorting in Dotnet Core projects.

The best use case of this library is Aspnet APIs when you need to get some filter or sort name or order name from the user and use that information to retrieve requested data from a repository or database or send pageable data to the user.

# Usage example

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
          .ApplyEverythingWithCount(filter) // Filter,Sort & Apply Paging 
          .ToList();

// pData.TotalItems => Count persons with 'John', First name
// pData.Items      => First 20 Persons with 'John', First Name
```


# Collaboration
Any collaboration to improve documentation and library is appreciated feel free to send pullrequest. <3





