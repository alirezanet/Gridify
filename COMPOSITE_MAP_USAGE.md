# Composite Map Feature - Usage Guide

## Overview

The Composite Map feature allows you to search across multiple properties with a single filter reference, automatically combining them with OR logic. This eliminates the need to construct complex filter strings on the frontend.

## Basic Usage

### Simple Composite Map

```csharp
var mapper = new GridifyMapper<Course>()
    .AddCompositeMap("student", 
        x => x.StudentAssignments.Select(s => s.StudentKey),
        x => x.StudentAssignments.Select(s => s.StudentKeyNavigation.Name));

// Frontend sends: student=xyz
// Generates: WHERE StudentKey = 'xyz' OR Name = 'xyz'
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
// Searches in Name, Description, and Id
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

// Usage - all Gridify operators are supported
var query1 = new GridifyQuery { Filter = "student=xyz" };        // Exact match
var query2 = new GridifyQuery { Filter = "student=*xyz" };       // Contains (=* operator)
var query3 = new GridifyQuery { Filter = "student!=xyz" };       // Not equal
var query4 = new GridifyQuery { Filter = "student>100" };        // Greater than

var results = dbContext.Courses
    .ApplyFiltering(query1, mapper)
    .ToList();
```

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
5. **Performance** - Generates efficient OR queries that work with query providers
6. **Composability** - Works with all Gridify features and operators

## Migration from Manual OR Filters

**Before:**
```csharp
// Frontend sends: students.key=*xyz|students.name=*xyz
mapper.AddMap("students.key", task => task.StudentAssignments.Select(row => row.StudentKey))
      .AddMap("students.name", task => task.StudentAssignments.Select(row => row.StudentKeyNavigation.Name));
```

**After:**
```csharp
// Frontend sends: students=*xyz
mapper.AddCompositeMap("students",
    task => task.StudentAssignments.Select(row => row.StudentKey),
    task => task.StudentAssignments.Select(row => row.StudentKeyNavigation.Name));
```

## Notes

- Composite maps are treated as a single logical field
- All expressions are combined with OR logic
- Shared convertors are applied to filter values before comparison
- The feature works with all query providers (Entity Framework, in-memory collections, etc.)
- Compile-time type safety is maintained for all expressions
- All Gridify operators are fully supported
