# Introduction

Greetings!

Gridify is an exceptional dynamic LINQ library designed to simplify the process of converting strings into LINQ queries. It offers remarkable performance and ease-of-use, making it effortless to apply filtering, sorting, and pagination utilizing text-based data.

## Features

Here are some notable features of Gridify:

- Fast and user-friendly
- Supports filtering, sorting, and pagination
- Enables conversion from strings to LINQ queries
- Supports nested queries and sub-collections
- Allows mapping from strings to objects
- Supports query compilation
- Supports DocumentDBs
- Compatible with collection indexes
- Custom operators
- Can be used with any collection that supports LINQ
- Compatible with object-mappers like AutoMapper
- Compatible with ORMs, particularly [Entity Framework](./extensions/entityframework)
- Supports [Elasticsearch](./extensions/elasticsearch) DSL query
- Javascript/Typescript [gridify-client](./extensions/gridify-client)

## Examples

To provide a better understanding of Gridify's functionality, we have prepared a few examples:

- [Using Gridify in API Controllers](../example/api-controller.md)
- More examples coming soon...

## Performance

Filtering is the most expensive feature in Gridify.
The following benchmark compares filtering in various popular dynamic LINQ libraries.
Interestingly, Gridify outperforms even Native LINQ in terms of speed.
It's worth mentioning that other features like Pagination and Sorting in Gridify have minimal impact on performance.

| Method           |         Mean |       Error |      StdDev |  Ratio |   Allocated | Alloc Ratio |
|------------------|-------------:|------------:|------------:|-------:|------------:|------------:|
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
   tr:nth-child(1) {
      color: #a8b1ff;
   }
</style>
