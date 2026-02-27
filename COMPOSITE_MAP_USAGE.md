# Composite Map Feature - Usage Examples

## Overview

The Composite Map feature allows you to search across multiple properties with a single filter reference, automatically combining them with OR logic. This is particularly useful when you want users to search a single field that should match against multiple entity properties.

## Basic Usage

### Simple Composite Map

```csharp
var mapper = new GridifyMapper<Course>()
    .AddCompositeMap("student", 
        x => x.StudentAssignments.Select(s => s.StudentKey),
        x => x.StudentAssignments.Select(s => s.StudentKeyNavigation.Name));

// Filter: student=xyz
// Will search for xyz in both StudentKey AND StudentKeyNavigation.Name
```

### With Different Property Types

```csharp
var mapper = new GridifyMapper<Product>()
    .AddCompositeMap("search",
        x => x.Name,
        x => x.Description,
        x => (object)x.Id);  // Cast to object for non-string types

// Filter: search=123
// Will search in Name, Description, and Id
```

## Supported Operations

✅ **Exact Match**
```csharp
var query = new GridifyQuery { Filter = "search=John" };
// Matches Name=John OR Tag=John
```

✅ **Combining with Other Filters**
```csharp
var query = new GridifyQuery { Filter = "search=John|id=5" };
// Matches (Name=John OR Tag=John) OR Id=5

var query = new GridifyQuery { Filter = "search=John,id>5" };
// Matches (Name=John OR Tag=John) AND Id>5
```

✅ **Multiple Property Types**
```csharp
.AddCompositeMap("search",
    x => x.Name,           // string
    x => (object)x.Id,     // int
    x => (object)x.MyGuid) // Guid
```

## Complete Example

```csharp
public class Course
{
    public int Id { get; set; }
    public string Name { get; set; }
    public List<StudentAssignment> StudentAssignments { get; set; }
}

public class StudentAssignment  
{
    public string StudentKey { get; set; }
    public Student StudentKeyNavigation { get; set; }
}

public class Student
{
    public string Key { get; set; }
    public string Name { get; set; }
}

// Setup
var mapper = new GridifyMapper<Course>()
    .AddCompositeMap("student",
        task => task.StudentAssignments.Select(row => row.StudentKey),
        task => task.StudentAssignments.Select(row => row.StudentKeyNavigation.Name));

// Usage
var query = new GridifyQuery { Filter = "student=xyz" };
var results = dbContext.Courses
    .ApplyFiltering(query, mapper)
    .ToList();

// SQL Generated will have: 
// WHERE StudentAssignments.StudentKey = 'xyz' 
//    OR StudentAssignments.StudentKeyNavigation.Name = 'xyz'
```

## Current Limitations

The following features are **not yet supported** for composite maps but may be added in future releases:

❌ **Wildcard Searches** - `*text*`, `text*`, `*text`  
❌ **Custom Converters** - Using `convertor` parameter  
❌ **Complex nested scenarios** - May need additional testing

For these scenarios, continue using individual `AddMap` calls combined with `|` (OR) operator in the filter string.

## API Reference

### AddCompositeMap

```csharp
IGridifyMapper<T> AddCompositeMap(
    string from,
    params Expression<Func<T, object?>>[] expressions)
```

**Parameters:**
- `from`: The field name to use in filters
- `expressions`: One or more property expressions to search across

**Returns:** The mapper instance for method chaining

**Example:**
```csharp
mapper.AddCompositeMap("search", x => x.Name, x => x.Email, x => x.Phone);
```

## Benefits

1. **Cleaner Frontend Code** - Send `search=value` instead of `name=value|email=value|phone=value`
2. **Backend Control** - Change which fields are searched without frontend changes
3. **Type Safety** - Compile-time checking of property expressions
4. **Performance** - Generates efficient OR queries in SQL/LINQ
5. **Composability** - Works with existing Gridify features

## Migration from Manual OR Filters

**Before:**
```csharp
// Frontend sends: students.key=*xyz|students.name=*xyz
mapper.AddMap("students.key", task => task.StudentAssignments.Select(row => row.StudentKey))
      .AddMap("students.name", task => task.StudentAssignments.Select(row => row.StudentKeyNavigation.Name));
```

**After:**
```csharp
// Frontend sends: students=xyz (exact match currently)
mapper.AddCompositeMap("students",
    task => task.StudentAssignments.Select(row => row.StudentKey),
    task => task.StudentAssignments.Select(row => row.StudentKeyNavigation.Name));
```

## Notes

- Composite maps are treated as a single logical field
- All expressions are combined with OR logic
- The feature works with EF Core, EF6, and in-memory collections
- Compile-time type safety is maintained for all expressions
