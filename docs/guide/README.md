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


|      Method |       Mean |    Error | Ratio |   Gen 0 |   Gen 1 | Allocated |
|------------ |-----------:|---------:|------:|--------:|--------:|----------:|
|  NativeLinQ |   823.8 us | 11.18 us |  1.00 |  4.8828 |  1.9531 |     35 KB |
| ***Gridify**   |   853.1 us | 13.88 us |  **1.03** |  6.8359 |  2.9297 |     43 KB |
| DynamicLinQ |   967.3 us |  6.65 us |  1.17 | 19.5313 |  9.7656 |    123 KB |
|       Sieve | 1,275.2 us |  5.62 us |  1.55 |  7.8125 |  3.9063 |     55 KB |
|         Fop | 3,480.2 us | 55.81 us |  4.23 | 54.6875 | 27.3438 |    343 KB |

::: details
BenchmarkDotNet=v0.13.1, OS=Windows 10.0.22000
11th Gen Intel Core i5-11400F 2.60GHz, 1 CPU, 12 logical and 6 physical cores
.NET SDK=6.0.100
  [Host]     : .NET 6.0.0 (6.0.21.52210), X64 RyuJIT
  DefaultJob : .NET 6.0.0 (6.0.21.52210), X64 RyuJIT
:::
