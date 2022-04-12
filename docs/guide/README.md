# Introduction

Gridify is a dynamic LINQ library that converts your *string* to a LINQ query in the easiest way possible with excellent performance.
it also provides an easy way to apply Filtering, Sorting, and Pagination using text-based data.

## Features

- It's Fast and easy to use
- Supports filtering, sorting, and pagination
- Supports `string` to LINQ conversion
- Supports nested queries and sub-collections
- Supports `string` to `object` mapping
- Supports query compilation
- Supports collection indexes
- Custom Operators
- Compatible with ORMs specially Entity Framework
- Can be used on every collection that LINQ supports
- Compatible with Object-mappers like AutoMapper

## Examples

To better illustrate how Gridify works, I have prepared a few examples.

Be sure to check out these examples

- [Using Gridify in API Controllers](../example/api-controller.md)
- Coming soon ...

## Performance

Filtering is the most expensive feature in gridify. the following benchmark is comparing filtering in the most known dynamic LINQ libraries. As you can see, gridify has the closest result to the native LINQ.

| Method           |         Mean |       Error |  Ratio |     Gen 0 |     Gen 1 | Allocated |
|------------------|-------------:|------------:|-------:|----------:|----------:|----------:|
| Native LINQ      |     806.3 us |     4.89 us |   1.00 |    4.8828 |    1.9531 |     35 KB |
| Gridify          |     839.6 us |     5.69 us |   1.04 |    5.8594 |    2.9297 |     39 KB |
| DynamicLinq      |     973.8 us |     8.65 us |   1.21 |   19.5313 |    9.7656 |    123 KB |
| Sieve            |   1,299.7 us |    12.74 us |   1.61 |    7.8125 |    3.9063 |     53 KB |
| Fop              |   3,498.6 us |    29.45 us |   4.34 |   54.6875 |   27.3438 |    348 KB |
| CSharp Scripting | 231,510.6 us | 4,406.95 us | 287.13 | 3000.0000 | 1000.0000 | 24,198 KB |

::: details
BenchmarkDotNet=v0.13.1, OS=Windows 10.0.22000
11th Gen Intel Core i5-11400F 2.60GHz, 1 CPU, 12 logical and 6 physical cores
.NET SDK=6.0.100
[Host]     : .NET 6.0.0 (6.0.21.52210), X64 RyuJIT
DefaultJob : .NET 6.0.0 (6.0.21.52210), X64 RyuJIT

This Benchmark is available [Here](https://github.com/alirezanet/Gridify/blob/master/benchmark/LibraryComparisionFilteringBenchmark.cs)
:::


<style scoped>
   tr:nth-child(2) {
      color: #42b983;
   }
</style>


