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

### With Shared Convertor

```csharp
var mapper = new GridifyMapper<Product>()
    .AddCompositeMap("search", 
        value => value.ToUpper(),  // Shared convertor for all expressions
        x => x.Name.ToUpper(),
        x => x.Description.ToUpper());

// Filter: search=phone
// Converts "phone" to "PHONE" and searches across both properties
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

✅ **Exact Match** (`=`)
```csharp
var query = new GridifyQuery { Filter = "search=John" };
// Matches Name=John OR Tag=John
```

✅ **Not Equal** (`!=`)
```csharp
var query = new GridifyQuery { Filter = "search!=John" };
// Matches Name!=John OR Tag!=John
```

✅ **Greater Than** (`>`)
```csharp
var query = new GridifyQuery { Filter = "search>3" };
// Matches Id>3 (on numeric properties)
```

✅ **Less Than** (`<`)
```csharp
var query = new GridifyQuery { Filter = "search<3" };
// Matches Id<3
```

✅ **Greater Or Equal** (`>=`)
```csharp
var query = new GridifyQuery { Filter = "search>=4" };
// Matches Id>=4
```

✅ **Less Or Equal** (`<=`)
```csharp
var query = new GridifyQuery { Filter = "search<=2" };
// Matches Id<=2
```

✅ **Starts With** (`^`)
```csharp
var query = new GridifyQuery { Filter = "search^John" };
// Matches Name starting with "John" OR Tag starting with "John"
```

✅ **Ends With** (`$`)
```csharp
var query = new GridifyQuery { Filter = "search$son" };
// Matches Name ending with "son" OR Tag ending with "son"
```

✅ **Combining with Other Filters**
```csharp
var query = new GridifyQuery { Filter = "search=John|id=5" };
// Matches (Name=John OR Tag=John) OR Id=5

var query = new GridifyQuery { Filter = "search=John,id>5" };
// Matches (Name=John OR Tag=John) AND Id>5
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

❌ **Wildcard Contains Searches** - `*text*` returns 0 results  
❌ **Custom Operators** - Only built-in operators are currently tested

For these scenarios, continue using individual `AddMap` calls combined with `|` (OR) operator in the filter string.

## API Reference

### AddCompositeMap

```csharp
// Without convertor
IGridifyMapper<T> AddCompositeMap(
    string from,
    params Expression<Func<T, object?>>[] expressions)

// With convertor
IGridifyMapper<T> AddCompositeMap(
    string from,
    Func<string, object>? convertor,
    params Expression<Func<T, object?>>[] expressions)
```

**Parameters:**
- `from`: The field name to use in filters
- `convertor`: Optional shared value converter function applied to filter values
- `expressions`: One or more property expressions to search across

**Returns:** The mapper instance for method chaining

**Examples:**
```csharp
// Simple usage
mapper.AddCompositeMap("search", x => x.Name, x => x.Email, x => x.Phone);

// With convertor
mapper.AddCompositeMap("search", 
    val => val.ToUpper(), 
    x => x.Name.ToUpper(), 
    x => x.Email.ToUpper());
```

## Benefits

1. **Cleaner Frontend Code** - Send `search=value` instead of `name=value|email=value|phone=value`
2. **Backend Control** - Change which fields are searched without frontend changes
3. **Type Safety** - Compile-time checking of property expressions
4. **Shared Convertors** - Apply value transformation across all properties
5. **Performance** - Generates efficient OR queries in SQL/LINQ
6. **Composability** - Works with existing Gridify features

## Migration from Manual OR Filters

**Before:**
```csharp
// Frontend sends: students.key=xyz|students.name=xyz
mapper.AddMap("students.key", task => task.StudentAssignments.Select(row => row.StudentKey))
      .AddMap("students.name", task => task.StudentAssignments.Select(row => row.StudentKeyNavigation.Name));
```

**After:**
```csharp
// Frontend sends: students=xyz
mapper.AddCompositeMap("students",
    task => task.StudentAssignments.Select(row => row.StudentKey),
    task => task.StudentAssignments.Select(row => row.StudentKeyNavigation.Name));
```

## Notes

- Composite maps are treated as a single logical field
- All expressions are combined with OR logic
- Shared convertors are applied to filter values before comparison
- The feature works with EF Core, EF6, and in-memory collections
- Compile-time type safety is maintained for all expressions
- Most Gridify operators are supported (=, !=, >, <, >=, <=, ^, $)
