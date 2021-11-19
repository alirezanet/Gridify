using System;
using System.Collections.Generic;
using System.Linq;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Order;
using Gridify;
using Gridify.Tests;

namespace Benchmarks
{
   [MemoryDiagnoser]
   [RPlotExporter]
   [Orderer(SummaryOrderPolicy.FastestToSlowest)]
   public class QueryBuilderBuildBenchmark
   {
      private readonly IEnumerable<TestClass> _data;
      private static readonly Consumer Consumer = new();
      private readonly Func<IEnumerable<TestClass>, IEnumerable<TestClass>> CompiledFunc;
      private readonly Func<IQueryable<TestClass>, IQueryable<TestClass>> QueryableFunc;
      private readonly Func<TestClass, bool> CompileFiltering;


      public QueryBuilderBuildBenchmark()
      {
         _data = LibraryComparisionFilteringBenchmark.GetSampleData().ToArray();

         var builder = new QueryBuilder<TestClass>()
            .AddOrderBy("name")
            .AddCondition("id>2")
            .AddCondition("name=*a");

         CompiledFunc = builder.BuildCompiled();
         QueryableFunc = builder.Build();
         CompileFiltering = builder.BuildFilteringExpression().Compile();

         if (AllSame(CompiledFunc(_data).Count(), QueryableFunc(_data.AsQueryable()).Count(), _data.Where(CompileFiltering).Count()) &&
             AllSame(CompiledFunc(_data).First().Id, QueryableFunc(_data.AsQueryable()).First().Id, _data.Where(CompileFiltering).First().Id) &&
             AllSame(CompiledFunc(_data).Last().Id, QueryableFunc(_data.AsQueryable()).Last().Id, _data.Where(CompileFiltering).Last().Id) &&
             CompiledFunc(_data).Count() < 2)
         {
            throw new Exception("MISS MATCH OUTPUT");
         }
      }

      [Benchmark]
      public void BuildCompiled()
      {
         CompiledFunc(_data).Consume(Consumer);
      }

      [Benchmark] // this method is only for filtering operations
      public void UseGetFilteringExpression()
      {
         _data.Where(CompileFiltering).Consume(Consumer);
      }

      [Benchmark]
      public void Build()
      {
         QueryableFunc(_data.AsQueryable()).Consume(Consumer);
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
}
