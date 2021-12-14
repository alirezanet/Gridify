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
public class QueryBuilderBuildBenchmark
{
   private readonly IEnumerable<TestClass> _data;
   private static readonly Consumer Consumer = new();
   private readonly Func<IEnumerable<TestClass>, IEnumerable<TestClass>> BuildCompiledFuc;
   private readonly Func<IQueryable<TestClass>, IQueryable<TestClass>> BuildFunc;
   private readonly Func<TestClass, bool> BuildFilteringExpressionFunc;
   private readonly Func<IEnumerable<TestClass>, Paging<TestClass>> BuildWithPagingCompiledFunc;
   private readonly Func<IQueryable<TestClass>, Paging<TestClass>> BuildWithPagingFunc;


   public QueryBuilderBuildBenchmark()
   {
      _data = LibraryComparisionFilteringBenchmark.GetSampleData().ToArray();

      var builder = new QueryBuilder<TestClass>()
         .AddCondition("id>2")
         .AddCondition("name=*a");

      BuildCompiledFuc = builder.BuildCompiled();
      BuildFunc = builder.Build();
      BuildFilteringExpressionFunc = builder.BuildFilteringExpression().Compile();
      BuildWithPagingCompiledFunc = builder.BuildWithPagingCompiled();
      BuildWithPagingFunc = builder.BuildWithPaging();

      TestOutputs();
   }

   [Benchmark(Baseline = true)] // this method is only for filtering operations
   public void BuildFilteringExpression()
   {
      _data.Where(BuildFilteringExpressionFunc).Consume(Consumer);
   }

   [Benchmark]
   public void Build()
   {
      BuildFunc(_data.AsQueryable()).Consume(Consumer);
   }

   [Benchmark]
   public void BuildCompiled()
   {
      BuildCompiledFuc(_data).Consume(Consumer);
   }

   [Benchmark]
   public void BuildWithPaging()
   {
      BuildWithPagingFunc(_data.AsQueryable()).Data.Consume(Consumer);
   }

   [Benchmark]
   public void BuildWithPagingCompiled()
   {
      BuildWithPagingCompiledFunc(_data.AsQueryable()).Data.Consume(Consumer);
   }

   private void TestOutputs()
   {
      if (AllSame(BuildCompiledFuc(_data).Count(), BuildFunc(_data.AsQueryable()).Count(), _data.Where(BuildFilteringExpressionFunc).Count()) &&
          AllSame(BuildCompiledFuc(_data).First().Id, BuildFunc(_data.AsQueryable()).First().Id,
             _data.Where(BuildFilteringExpressionFunc).First().Id) &&
          AllSame(BuildCompiledFuc(_data).Last().Id, BuildFunc(_data.AsQueryable()).Last().Id,
             _data.Where(BuildFilteringExpressionFunc).Last().Id) &&
          BuildCompiledFuc(_data).Count() < 2)
      {
         throw new Exception("MISS MATCH OUTPUT");
      }
   }

   private static bool AllSame<T>(params T[] items)
   {
      var first = true;
      T comparand = default;
      foreach (var i in items)
      {
         if (first) comparand = i;
         else if (!i.Equals(comparand)) return false;
         first = false;
      }

      return true;
   }
}