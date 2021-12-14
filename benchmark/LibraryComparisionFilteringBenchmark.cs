using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using BenchmarkDotNet.Order;
using Fop;
using Fop.FopExpression;
using Gridify;
using Gridify.Tests;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.Extensions.Options;
using Sieve.Models;
using Sieve.Services;

namespace Benchmarks;

[MemoryDiagnoser]
[RPlotExporter]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
public class LibraryComparisionFilteringBenchmark
{
   private static readonly Consumer Consumer = new();
   private TestClass[] _data;
   private ScriptOptions _options;
   private SieveProcessor _processor;
   private GridifyMapper<TestClass> _gm;

   private IQueryable<TestClass> Ds => _data.AsQueryable();

   [GlobalSetup]
   public void Setup()
   {
      _data = GetSampleData().ToArray();

      // Sieve
      _processor = new SieveProcessor(new OptionsWrapper<SieveOptions>(new SieveOptions()));

      // gridify
      _gm = new GridifyMapper<TestClass>(true);

      // CSharpScripting
      _options = ScriptOptions.Default.AddReferences(typeof(TestClass).Assembly);
   }


   [Benchmark(Baseline = true)]
   public void Native_LINQ()
   {
      Ds.Where(q => q.Name.Contains('a')).Consume(Consumer);
      Ds.Where(q => q.Id > 5).Consume(Consumer);
      Ds.Where(q => q.Name == "Ali").Consume(Consumer);
   }

   [Benchmark]
   public void Gridify()
   {
      Ds.ApplyFiltering("Name=*a", _gm).Consume(Consumer);
      Ds.ApplyFiltering("Id>5", _gm).Consume(Consumer);
      Ds.ApplyFiltering("Name=Ali", _gm).Consume(Consumer);
   }

   [Benchmark]
   public void Fop()
   {
      // fop doesn't have filtering only feature?
      Ds.ApplyFop(FopExpressionBuilder<TestClass>.Build("Name~=a", "Name", 1, 1000)).Item1.Consume(Consumer);
      Ds.ApplyFop(FopExpressionBuilder<TestClass>.Build("Id>5", "Name", 1, 1000)).Item1.Consume(Consumer);
      Ds.ApplyFop(FopExpressionBuilder<TestClass>.Build("Name==Ali", "Name", 1, 1000)).Item1.Consume(Consumer);
   }

   [Benchmark]
   public void DynamicLinq()
   {
      Ds.Where("Name.Contains(@0)", "a").Consume(Consumer);
      Ds.Where("Id > (@0)", "5").Consume(Consumer);
      Ds.Where("Name==(@0)", "Ali").Consume(Consumer);
   }

   [Benchmark]
   public void CSharp_Scripting()
   {
      // Is there any non-async way to do this?
      Ds.Where(CSharpScript.EvaluateAsync<Expression<Func<TestClass, bool>>>
         ("q => q.Name.Contains('a')", _options).Result).Consume(Consumer);

      Ds.Where(CSharpScript.EvaluateAsync<Expression<Func<TestClass, bool>>>
         ("q => q.Id > 5", _options).Result).Consume(Consumer);

      Ds.Where(CSharpScript.EvaluateAsync<Expression<Func<TestClass, bool>>>
         ("q => q.Name == \"Ali\"", _options).Result).Consume(Consumer);
   }

   [Benchmark]
   public void Sieve()
   {
      _processor.Apply(new SieveModel { Filters = "Name@=a" }, Ds, applySorting: false, applyPagination: false).Consume(Consumer);
      _processor.Apply(new SieveModel { Filters = "Id>5" }, Ds, applySorting: false, applyPagination: false).Consume(Consumer);
      _processor.Apply(new SieveModel { Filters = "Name==Ali" }, Ds, applySorting: false, applyPagination: false).Consume(Consumer);
   }


   public static IEnumerable<TestClass> GetSampleData()
   {
      var lst = new List<TestClass>();
      lst.Add(new TestClass(1, "John", null, Guid.NewGuid(), DateTime.Now));
      lst.Add(new TestClass(2, "Bob", null, Guid.NewGuid(), DateTime.UtcNow));
      lst.Add(new TestClass(3, "Jack", (TestClass)lst[0].Clone(), Guid.Empty, DateTime.Now.AddDays(2)));
      lst.Add(new TestClass(4, "Rose", null, Guid.Parse("e2cec5dd-208d-4bb5-a852-50008f8ba366")));
      lst.Add(new TestClass(5, "Ali", null));
      lst.Add(new TestClass(6, "Hamid", (TestClass)lst[0].Clone(), Guid.Parse("de12bae1-93fa-40e4-92d1-2e60f95b468c")));
      lst.Add(new TestClass(7, "Hasan", (TestClass)lst[1].Clone()));
      lst.Add(new TestClass(8, "Farhad", (TestClass)lst[2].Clone(), Guid.Empty));
      lst.Add(new TestClass(9, "Sara", null));
      lst.Add(new TestClass(10, "Jorge", null));
      lst.Add(new TestClass(11, "joe", null));
      lst.Add(new TestClass(12, "jimmy", (TestClass)lst[0].Clone()));
      lst.Add(new TestClass(13, "Nazanin", null));
      lst.Add(new TestClass(14, "Reza", null));
      lst.Add(new TestClass(15, "Korosh", (TestClass)lst[0].Clone()));
      lst.Add(new TestClass(16, "Kamran", (TestClass)lst[1].Clone()));
      lst.Add(new TestClass(17, "Saeid", (TestClass)lst[2].Clone()));
      lst.Add(new TestClass(18, "jessi==ca", null));
      lst.Add(new TestClass(19, "Ped=ram", null));
      lst.Add(new TestClass(20, "Peyman!", null));
      lst.Add(new TestClass(21, "Fereshte", null));
      lst.Add(new TestClass(22, "LIAM", null));
      lst.Add(new TestClass(22, @"\Liam", null));
      lst.Add(new TestClass(23, "LI | AM", null));
      lst.Add(new TestClass(24, "(LI,AM)", null));
      return lst;
   }
}

// BenchmarkDotNet=v0.13.1, OS=Windows 10.0.22000
// 11th Gen Intel Core i5-11400F 2.60GHz, 1 CPU, 12 logical and 6 physical cores
//    .NET SDK=6.0.100
//    [Host]     : .NET 6.0.0 (6.0.21.52210), X64 RyuJIT
// DefaultJob : .NET 6.0.0 (6.0.21.52210), X64 RyuJIT
//
//
//    |           Method |         Mean |       Error |      StdDev |  Ratio | RatioSD |     Gen 0 |     Gen 1 | Allocated |
//    |----------------- |-------------:|------------:|------------:|-------:|--------:|----------:|----------:|----------:|
//    |      Native_LINQ |     806.3 us |     4.89 us |     4.57 us |   1.00 |    0.00 |    4.8828 |    1.9531 |     35 KB |
//    |          Gridify |     839.6 us |     5.69 us |     4.75 us |   1.04 |    0.01 |    5.8594 |    2.9297 |     39 KB |
//    |      DynamicLinq |     973.8 us |     8.65 us |     6.75 us |   1.21 |    0.01 |   19.5313 |    9.7656 |    123 KB |
//    |            Sieve |   1,299.7 us |    12.74 us |    11.29 us |   1.61 |    0.02 |    7.8125 |    3.9063 |     53 KB |
//    |              Fop |   3,498.6 us |    29.45 us |    26.11 us |   4.34 |    0.03 |   54.6875 |   27.3438 |    348 KB |
//    | CSharp_Scripting | 231,510.6 us | 4,406.95 us | 4,122.26 us | 287.13 |    5.12 | 3000.0000 | 1000.0000 | 24,198 KB |
