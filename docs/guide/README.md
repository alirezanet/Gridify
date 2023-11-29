# Introduction

Gridify is a dynamic LINQ library that simplifies the process of converting strings to LINQ queries. With exceptional
performance and ease-of-use, Gridify makes it effortless to apply filtering, sorting, and pagination using text-based
data.

Gridify.Elasticsearch is an extension of Gridify, that provides an ability to generate Elasticsearch DSL queries. Read more about Gridify.Elasticsearch in [a separate thread of the documentation](./elasticsearch/).

## Features

- Fast and easy to use
- Supports filtering, sorting, and pagination
- Supports `string` to LINQ conversion
- Supports nested queries and sub-collections
- Supports `string` to `object` mapping
- Supports query compilation
- Supports collection indexes
- Custom Operators
- Compatible with ORMs, especially Entity Framework
- Can be used on every collection that LINQ supports
- Compatible with object-mappers like AutoMapper

## Examples

To better illustrate how Gridify works, we've prepared a few examples:

- [Using Gridify in API Controllers](../example/api-controller.md)
- Coming soon ...

## Performance

Filtering is the most expensive feature in Gridify.
The following benchmark compares filtering in various popular dynamic LINQ libraries.
Interestingly, Gridify outperforms even Native LINQ in terms of speed.
It's worth mentioning that other features like Pagination and Sorting in Gridify have minimal impact on performance.



| Method           | Mean         | Error       | StdDev      | Ratio  | Allocated   | Alloc Ratio |
|----------------- |-------------:|------------:|------------:|-------:|------------:|------------:|
| Gridify          |     599.8 us |     2.76 us |     2.45 us |   0.92 |    36.36 KB |        1.11 |
| Native_LINQ      |     649.9 us |     2.55 us |     2.38 us |   1.00 |    32.74 KB |        1.00 |
| DynamicLinq      |     734.8 us |    13.90 us |    13.01 us |   1.13 |    119.4 KB |        3.65 |
| Sieve            |   1,190.9 us |     7.41 us |     6.93 us |   1.83 |    53.05 KB |        1.62 |
| Fop              |   2,637.6 us |     8.59 us |     7.61 us |   4.06 |   321.57 KB |        9.82 |
| CSharp_Scripting | 216,863.8 us | 4,295.66 us | 6,021.92 us | 336.64 | 23660.26 KB |      722.71 |

::: details
BenchmarkDotNet v0.13.10, Windows 11 (10.0.22621.2715/22H2/2022Update/SunValley2)
12th Gen Intel Core i7-12800H, 1 CPU, 20 logical and 14 physical cores
.NET SDK 8.0.100
[Host]     : .NET 8.0.0 (8.0.23.53103), X64 RyuJIT AVX2

This Benchmark is
available [Here](https://github.com/alirezanet/Gridify/blob/master/benchmark/LibraryComparisionFilteringBenchmark.cs)
:::

<style scoped>
   tr:nth-child(2) {
      color: #42b983;
   }
</style>
