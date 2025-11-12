# Introduction

Greetings!

Gridify is a dynamic LINQ library designed to simplify the process of converting strings into LINQ queries. It offers remarkable performance and ease-of-use, making it effortless to apply filtering, sorting, and pagination utilizing text-based data.

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

Filtering remains the most expensive operation in Gridify.

In .NET 8, Gridify was slightly faster than native LINQ, but with .NET 10 improvements to LINQ itself,
native performance has caught up.

The latest benchmark shows Gridify performing nearly identically to native LINQ,
while still clearly ahead of other dynamic LINQ libraries.

Pagination and sorting continue to have minimal performance impact.

| Method                | Mean       | Error     | StdDev    | Ratio  | RatioSD | Allocated   | Alloc Ratio |
|---------------------- |-----------:|----------:|----------:|-------:|--------:|------------:|------------:|
| Native_LINQ           |   1.049 ms | 0.0047 ms | 0.0041 ms |   1.00 |    0.01 |    32.02 KB |        1.00 |
| Gridify               |   1.079 ms | 0.0071 ms | 0.0066 ms |   1.03 |    0.01 |    34.76 KB |        1.09 |
| Gridify_WithoutMapper |   1.090 ms | 0.0074 ms | 0.0069 ms |   1.04 |    0.01 |    40.37 KB |        1.26 |
| Sieve                 |   1.216 ms | 0.0103 ms | 0.0097 ms |   1.16 |    0.01 |    44.21 KB |        1.38 |
| DynamicLinq           |   1.284 ms | 0.0088 ms | 0.0082 ms |   1.22 |    0.01 |    92.42 KB |        2.89 |
| Fop                   |   4.749 ms | 0.0445 ms | 0.0416 ms |   4.53 |    0.04 |   284.77 KB |        8.89 |
| CSharp_Scripting      | 224.708 ms | 4.4534 ms | 7.5621 ms | 214.24 |    7.17 | 23303.11 KB |      727.67 |

::: details
```
BenchmarkDotNet v0.15.6, Windows 11 (10.0.26200.7171)
AMD Ryzen 7 7800X3D 4.20GHz, 1 CPU, 8 logical and 8 physical cores
.NET SDK 10.0.100
[Host]     : .NET 10.0.0 (10.0.0, 10.0.25.52411), X64 RyuJIT x86-64-v4
DefaultJob : .NET 10.0.0 (10.0.0, 10.0.25.52411), X64 RyuJIT x86-64-v4
```

This Benchmark is
available [Here](https://github.com/alirezanet/Gridify/blob/master/benchmark/LibraryComparisionFilteringBenchmark.cs)
:::

<style scoped>
   tr:nth-child(2) {
      color: #a8b1ff;
   }
</style>
