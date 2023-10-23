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

Filtering can be the most expensive feature in Gridify. The following benchmark compares filtering in the most
well-known dynamic LINQ libraries. As you can see, Gridify has the closest result to native LINQ:

| Method           |         Mean |       Error |      StdDev | RatioSD |   Allocated | Alloc Ratio |
|------------------|-------------:|------------:|------------:|--------:|------------:|------------:|
| Native LINQ      |     651.2 us |     6.89 us |     6.45 us |    0.00 |    32.74 KB |        1.00 |
| Gridify          |     689.1 us |    10.70 us |    11.45 us |    0.02 |    36.85 KB |        1.13 |
| DynamicLinq      |     829.3 us |    10.98 us |     9.17 us |    0.01 |   119.29 KB |        3.64 |
| Sieve            |   1,357.1 us |    15.60 us |    13.83 us |    0.03 |    54.03 KB |        1.65 |
| Fop              |   2,931.5 us |    38.05 us |    33.73 us |    0.08 |    322.9 KB |        9.86 |
| CSharp_Scripting | 191,795.6 us | 3,548.34 us | 6,577.07 us |    9.26 | 23697.48 KB |      723.87 |

::: details
BenchmarkDotNet v0.13.8, Windows 11 (10.0.22621.2283/22H2/2022Update/SunValley2)
12th Gen Intel Core i7-12800H, 1 CPU, 20 logical and 14 physical cores
.NET SDK 8.0.100-preview.1.23115.2
[Host]     : .NET 7.0.11 (7.0.1123.42427), X64 RyuJIT AVX2
DefaultJob : .NET 7.0.11 (7.0.1123.42427), X64 RyuJIT AVX2

This Benchmark is
available [Here](https://github.com/alirezanet/Gridify/blob/master/benchmark/LibraryComparisionFilteringBenchmark.cs)
:::

<style scoped>
   tr:nth-child(2) {
      color: #42b983;
   }
</style>
