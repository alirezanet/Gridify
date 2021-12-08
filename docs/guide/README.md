# Introduction

Gridify is a dynamic LINQ library that converts your *strings* to a LINQ query in the easiest way possible with excellent performance.
it also provides an easy way to apply Filtering, Sorting, and pagination using text-based data.


## How It Works

To better illustrate how Gridify works, I have prepared a few examples.

Be sure to check out these examples
- [Using Gridify in API Controllers](../example/api-controller.md)
- Coming soon ...

## Performance

Filtering is the most expensive feature in gridify. the following benchmark is comparing filtering in the most known dynamic linq libraries. As you can see, gridify has the closest result to the native linq.


|      Method |       Mean |    Error |   StdDev | Ratio |   Gen 0 |   Gen 1 | Allocated |
|------------ |-----------:|---------:|---------:|------:|--------:|--------:|----------:|
| Native LINQ |   740.9 us |  7.80 us |  6.92 us |  1.00 |  5.8594 |  2.9297 |     37 KB |
| **Gridify*** |   762.6 us | 10.06 us |  9.41 us |  1.03 |  5.8594 |  2.9297 |     39 KB |
| DynamicLinq |   902.1 us | 11.56 us | 10.81 us |  1.22 | 19.5313 |  9.7656 |    122 KB |
|       Sieve |   977.9 us |  6.80 us |  6.37 us |  1.32 |  7.8125 |  3.9063 |     54 KB |
|         Fop | 2,959.8 us | 39.11 us | 36.58 us |  3.99 | 46.8750 | 23.4375 |    306 KB |

::: details
BenchmarkDotNet=v0.13.0, OS=Windows 10.0.19043.1237 (21H1/May2021Update)
11th Gen Intel Core i5-11400F 2.60GHz, 1 CPU, 12 logical and 6 physical cores
.NET SDK=5.0.301
[Host]     : .NET 5.0.7 (5.0.721.25508), X64 RyuJIT
DefaultJob : .NET 5.0.7 (5.0.721.25508), X64 RyuJIT
:::
