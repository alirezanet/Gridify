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
   public void Gridify_WithoutMapper()
   {
      Ds.ApplyFiltering("Name=*a").Consume(Consumer);
      Ds.ApplyFiltering("Id>5").Consume(Consumer);
      Ds.ApplyFiltering("Name=Ali").Consume(Consumer);
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

// BenchmarkDotNet v0.13.10, Windows 11 (10.0.22621.2715/22H2/2022Update/SunValley2)
// 12th Gen Intel Core i7-12800H, 1 CPU, 20 logical and 14 physical cores
//    .NET SDK 8.0.100
//    [Host]     : .NET 8.0.0 (8.0.23.53103), X64 RyuJIT AVX2
// DefaultJob : .NET 8.0.0 (8.0.23.53103), X64 RyuJIT AVX2
//
//
//    | Method           | Mean         | Error       | StdDev      | Ratio  | RatioSD | Gen0      | Gen1     | Allocated   | Alloc Ratio |
//    |----------------- |-------------:|------------:|------------:|-------:|--------:|----------:|---------:|------------:|------------:|
//    | Gridify          |     599.8 us |     2.76 us |     2.45 us |   0.92 |    0.01 |    2.9297 |   1.9531 |    36.36 KB |        1.11 |
//    | Native_LINQ      |     649.9 us |     2.55 us |     2.38 us |   1.00 |    0.00 |    1.9531 |   0.9766 |    32.74 KB |        1.00 |
//    | DynamicLinq      |     734.8 us |    13.90 us |    13.01 us |   1.13 |    0.02 |    7.8125 |   5.8594 |    119.4 KB |        3.65 |
//    | Sieve            |   1,190.9 us |     7.41 us |     6.93 us |   1.83 |    0.01 |    3.9063 |   1.9531 |    53.05 KB |        1.62 |
//    | Fop              |   2,637.6 us |     8.59 us |     7.61 us |   4.06 |    0.02 |   23.4375 |  19.5313 |   321.57 KB |        9.82 |
//    | CSharp_Scripting | 216,863.8 us | 4,295.66 us | 6,021.92 us | 336.64 |   10.49 | 1500.0000 | 500.0000 | 23660.26 KB |      722.71 |
