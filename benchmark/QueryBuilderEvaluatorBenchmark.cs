using System;
using System.Collections.Generic;
using System.Linq;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Order;
using Gridify;
using Gridify.Tests;

namespace Benchmarks;

[MemoryDiagnoser]
[RPlotExporter]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
public class QueryBuilderEvaluatorBenchmark
{
   private readonly IEnumerable<TestClass> _data;
   // private static readonly Consumer Consumer = new();
   private readonly Func<IQueryable<TestClass>,bool> BuildEvaluatorFunc;
   private readonly  Func<IEnumerable<TestClass>,bool> BuildCompiledEvaluatorFunc;

   public QueryBuilderEvaluatorBenchmark()
   {
      _data = LibraryComparisionFilteringBenchmark.GetSampleData().ToArray();

      var builder = new QueryBuilder<TestClass>()
         .AddCondition("id>2")
         .AddCondition("name=*a");

      BuildEvaluatorFunc = builder.BuildEvaluator();
      BuildCompiledEvaluatorFunc = builder.BuildCompiledEvaluator();
   }

   [Benchmark]
   public void BuildEvaluator()
   {
      BuildEvaluatorFunc(_data.AsQueryable());
   }

   [Benchmark]
   public void BuildCompiledEvaluator()
   {
      BuildCompiledEvaluatorFunc(_data);
   }

}

/* Last Run:
BenchmarkDotNet=v0.13.0, OS=Windows 10.0.22000
11th Gen Intel Core i5-11400F 2.60GHz, 1 CPU, 12 logical and 6 physical cores
   .NET SDK=6.0.100
   [Host]     : .NET 5.0.7 (5.0.721.25508), X64 RyuJIT
DefaultJob : .NET 5.0.7 (5.0.721.25508), X64 RyuJIT


   |                 Method |         Mean |       Error |      StdDev |  Gen 0 |  Gen 1 | Gen 2 | Allocated |
   |----------------------- |-------------:|------------:|------------:|-------:|-------:|------:|----------:|
   | BuildCompiledEvaluator |     104.3 ns |     2.07 ns |     2.38 ns | 0.0305 |      - |     - |     192 B |
   |         BuildEvaluator | 466,066.5 ns | 4,392.92 ns | 4,109.14 ns | 3.4180 | 1.4648 |     - |  23,475 B |
*/
