using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using AutoBogus;
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
public class LibraryComparisionFilteringBenchmark2
{
   [Params(1, 10, 100, 1000, 10000)]
   // ReSharper disable once UnassignedField.Global
   public int N;

   private static readonly Consumer Consumer = new();
   private List<TestClass> _data;
   private ScriptOptions _options;
   private SieveProcessor _processor;
   private GridifyMapper<TestClass> _gm;

   private IQueryable<TestClass> Ds => _data.AsQueryable();

   [GlobalSetup]
   public void Setup()
   {
      _data = GetSampleData().ToList();
      _data = new AutoFaker<TestClass>()
         .UseSeed(1368)
         .Generate(N).Concat(_data)
         .ToList();

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
   public void DynamicLinq()
   {
      Ds.Where("Name.Contains(@0)", "a").Consume(Consumer);
      Ds.Where("Id > (@0)", "5").Consume(Consumer);
      Ds.Where("Name==(@0)", "Ali").Consume(Consumer);
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
// .NET SDK 8.0.100
//   [Host]     : .NET 8.0.0 (8.0.23.53103), X64 RyuJIT AVX2
//   DefaultJob : .NET 8.0.0 (8.0.23.53103), X64 RyuJIT AVX2
//
//
// | Method           | N     | Mean         | Error       | StdDev       | Median       | Ratio  | RatioSD | Gen0      | Gen1     | Allocated   | Alloc Ratio |
// |----------------- |------ |-------------:|------------:|-------------:|-------------:|-------:|--------:|----------:|---------:|------------:|------------:|
// | Gridify          | 1     |     615.6 us |    12.20 us |     16.70 us |     610.7 us |   0.96 |    0.03 |    2.9297 |   1.9531 |    36.68 KB |        1.11 |
// | Native_LINQ      | 1     |     645.3 us |     4.36 us |      4.07 us |     645.1 us |   1.00 |    0.00 |    1.9531 |   0.9766 |    33.07 KB |        1.00 |
// | DynamicLinq      | 1     |     712.4 us |     7.46 us |      6.61 us |     711.4 us |   1.10 |    0.01 |    7.8125 |   3.9063 |   119.58 KB |        3.62 |
// | Sieve            | 1     |   1,195.9 us |     4.97 us |      4.65 us |   1,197.1 us |   1.85 |    0.01 |    3.9063 |   1.9531 |    53.38 KB |        1.61 |
// | Fop              | 1     |   2,648.7 us |    14.91 us |     13.95 us |   2,645.1 us |   4.10 |    0.04 |   23.4375 |  19.5313 |   323.25 KB |        9.78 |
// | CSharp_Scripting | 1     | 211,812.2 us | 4,197.02 us |  8,573.41 us | 209,418.6 us | 342.32 |   11.46 | 1000.0000 |        - | 23659.02 KB |      715.50 |
// |                  |       |              |             |              |              |        |         |           |          |             |             |
// | Gridify          | 10    |     601.0 us |     3.10 us |      2.90 us |     600.6 us |   0.95 |    0.01 |    2.9297 |   1.9531 |    36.68 KB |        1.11 |
// | Native_LINQ      | 10    |     631.6 us |     2.38 us |      2.11 us |     631.0 us |   1.00 |    0.00 |    1.9531 |   0.9766 |    33.07 KB |        1.00 |
// | DynamicLinq      | 10    |     717.8 us |     4.18 us |      3.91 us |     718.3 us |   1.14 |    0.01 |    9.7656 |   5.8594 |   119.91 KB |        3.63 |
// | Sieve            | 10    |   1,189.3 us |     3.16 us |      2.95 us |   1,189.0 us |   1.88 |    0.01 |    3.9063 |   1.9531 |    53.59 KB |        1.62 |
// | Fop              | 10    |   2,636.4 us |    10.14 us |      9.49 us |   2,639.4 us |   4.17 |    0.01 |   23.4375 |  19.5313 |   324.28 KB |        9.81 |
// | CSharp_Scripting | 10    | 215,079.3 us | 4,216.38 us |  6,687.63 us | 212,348.4 us | 344.27 |   13.14 | 1500.0000 | 500.0000 | 23652.86 KB |      715.31 |
// |                  |       |              |             |              |              |        |         |           |          |             |             |
// | Gridify          | 100   |     606.1 us |     3.25 us |      3.04 us |     605.7 us |   0.94 |    0.01 |    2.9297 |   1.9531 |    36.68 KB |        1.11 |
// | Native_LINQ      | 100   |     642.3 us |     2.28 us |      2.13 us |     642.8 us |   1.00 |    0.00 |    1.9531 |   0.9766 |    33.07 KB |        1.00 |
// | DynamicLinq      | 100   |     725.6 us |     4.12 us |      3.85 us |     725.8 us |   1.13 |    0.01 |    9.7656 |   5.8594 |   119.92 KB |        3.63 |
// | Sieve            | 100   |   1,222.7 us |     4.10 us |      3.63 us |   1,221.9 us |   1.90 |    0.01 |    3.9063 |   1.9531 |    53.38 KB |        1.61 |
// | Fop              | 100   |   2,987.4 us |    58.00 us |     54.25 us |   2,976.7 us |   4.65 |    0.09 |   23.4375 |  19.5313 |   326.93 KB |        9.89 |
// | CSharp_Scripting | 100   | 231,329.8 us | 4,419.03 us | 12,318.50 us | 228,536.5 us | 381.97 |   22.97 | 1500.0000 | 500.0000 | 23654.77 KB |      715.37 |
// |                  |       |              |             |              |              |        |         |           |          |             |             |
// | Gridify          | 1000  |     692.7 us |    13.79 us |     14.76 us |     690.0 us |   0.96 |    0.01 |    2.9297 |   1.9531 |    36.68 KB |        1.11 |
// | Native_LINQ      | 1000  |     722.0 us |    13.87 us |     11.58 us |     717.7 us |   1.00 |    0.00 |    1.9531 |   0.9766 |    33.07 KB |        1.00 |
// | DynamicLinq      | 1000  |     800.0 us |    15.49 us |     15.91 us |     792.2 us |   1.11 |    0.03 |    9.7656 |   5.8594 |   119.74 KB |        3.62 |
// | Sieve            | 1000  |   1,256.9 us |    10.23 us |      9.57 us |   1,254.5 us |   1.74 |    0.02 |    3.9063 |   1.9531 |    53.38 KB |        1.61 |
// | Fop              | 1000  |   3,196.8 us |    39.37 us |     30.74 us |   3,192.3 us |   4.43 |    0.07 |   23.4375 |  15.6250 |   360.78 KB |       10.91 |
// | CSharp_Scripting | 1000  | 217,307.2 us | 3,013.25 us |  6,017.78 us | 216,025.5 us | 308.08 |    9.55 | 1500.0000 | 500.0000 | 23657.34 KB |      715.45 |
// |                  |       |              |             |              |              |        |         |           |          |             |             |
// | Gridify          | 10000 |   1,078.1 us |     5.66 us |      5.01 us |   1,077.2 us |   0.98 |    0.01 |    1.9531 |        - |    36.59 KB |        1.11 |
// | Native_LINQ      | 10000 |   1,093.9 us |     8.09 us |      7.57 us |   1,096.9 us |   1.00 |    0.00 |    1.9531 |        - |    33.02 KB |        1.00 |
// | DynamicLinq      | 10000 |   1,188.0 us |     9.59 us |      8.50 us |   1,186.7 us |   1.09 |    0.01 |    7.8125 |   3.9063 |   119.58 KB |        3.62 |
// | Sieve            | 10000 |   1,676.4 us |     9.56 us |      8.94 us |   1,678.8 us |   1.53 |    0.01 |    3.9063 |        - |     53.5 KB |        1.62 |
// | Fop              | 10000 |   5,134.3 us |    55.71 us |     52.11 us |   5,135.7 us |   4.69 |    0.06 |   46.8750 |  23.4375 |   661.42 KB |       20.03 |
// | CSharp_Scripting | 10000 | 218,899.3 us | 4,145.30 us |  6,693.89 us | 215,678.5 us | 203.68 |    7.02 | 1500.0000 | 500.0000 | 23657.06 KB |      716.41 |
