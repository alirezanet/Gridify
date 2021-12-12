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
public class GridifyMapperUsages
{
   private static readonly Consumer Consumer = new();
   private TestClass[] _data;
   private Func<TestClass, bool> compiled1;
   private Func<TestClass, bool> compiled2;
   private Func<TestClass, bool> compiled3;

   private IQueryable<TestClass> Ds => _data.AsQueryable();
   private IEnumerable<TestClass> EnumerableDs => _data.ToList();
   private IGridifyMapper<TestClass> ggm { get; set; }

   [GlobalSetup]
   public void Setup()
   {
      _data = GetSampleData().ToArray();

      ggm = new GridifyMapper<TestClass>().GenerateMappings();

      // compiled query (this is not included in our readme benchmarks)
      var gq1 = new GridifyQuery() { Filter = "Name=*a" };
      var gq2 = new GridifyQuery() { Filter = "Id>5" };
      var gq3 = new GridifyQuery() { Filter = "Name=Ali" };
      compiled1 = gq1.GetFilteringExpression(ggm).Compile();
      compiled2 = gq2.GetFilteringExpression(ggm).Compile();
      compiled3 = gq3.GetFilteringExpression(ggm).Compile();
   }


   [Benchmark(Baseline = true)]
   public void NativeLinQ()
   {
      Ds.Where(q => q.Name.Contains("a")).Consume(Consumer);
      Ds.Where(q => q.Id > 5).Consume(Consumer);
      Ds.Where(q => q.Name == "Ali").Consume(Consumer);
   }
   [Benchmark]
   public void Gridify_GlobalMapper()
   {
      Ds.ApplyFiltering("Name=*a", ggm).Consume(Consumer);
      Ds.ApplyFiltering("Id>5", ggm).Consume(Consumer);
      Ds.ApplyFiltering("Name=Ali", ggm).Consume(Consumer);
   }
   [Benchmark]
   public void Gridify_SingleMapper_Generated()
   {
      var gm = new GridifyMapper<TestClass>().GenerateMappings();
      Ds.ApplyFiltering("Name=*a", gm).Consume(Consumer);
      Ds.ApplyFiltering("Id>5", gm).Consume(Consumer);
      Ds.ApplyFiltering("Name=Ali", gm).Consume(Consumer);
   }

   [Benchmark]
   public void Gridify_SingleMapper_Manual()
   {
      var gm = new GridifyMapper<TestClass>()
         .AddMap("name")
         .AddMap("id");

      Ds.ApplyFiltering("Name=*a", gm).Consume(Consumer);
      Ds.ApplyFiltering("Id>5", gm).Consume(Consumer);
      Ds.ApplyFiltering("Name=Ali", gm).Consume(Consumer);
   }

   [Benchmark]
   public void Gridify_NoMapper()
   {
      Ds.ApplyFiltering("Name=*a").Consume(Consumer);
      Ds.ApplyFiltering("Id>5").Consume(Consumer);
      Ds.ApplyFiltering("Name=Ali").Consume(Consumer);
   }
   [Benchmark]
   public void Gridify_EachAction_Generated()
   {
      Ds.ApplyFiltering("Name=*a", new GridifyMapper<TestClass>().GenerateMappings()).Consume(Consumer);
      Ds.ApplyFiltering("Id>5", new GridifyMapper<TestClass>().GenerateMappings()).Consume(Consumer);
      Ds.ApplyFiltering("Name=Ali", new GridifyMapper<TestClass>().GenerateMappings()).Consume(Consumer);
   }

   // [Benchmark]
   // public void GridifyCompiled()
   // {
   //    EnumerableDs.Where(compiled1).Consume(Consumer);
   //    EnumerableDs.Where(compiled2).Consume(Consumer);
   //    EnumerableDs.Where(compiled3).Consume(Consumer);
   // }

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
// .NET SDK=6.0.100
//   [Host]     : .NET 6.0.0 (6.0.21.52210), X64 RyuJIT
//   DefaultJob : .NET 6.0.0 (6.0.21.52210), X64 RyuJIT
//
//
// |                         Method |     Mean |    Error |   StdDev | Ratio | RatioSD |  Gen 0 |  Gen 1 | Allocated |
// |------------------------------- |---------:|---------:|---------:|------:|--------:|-------:|-------:|----------:|
// |                     NativeLinQ | 826.9 us | 13.49 us | 12.61 us |  1.00 |    0.00 | 4.8828 | 1.9531 |     35 KB |
// |    Gridify_SingleMapper_Manual | 846.4 us |  8.64 us |  7.66 us |  1.02 |    0.02 | 5.8594 | 2.9297 |     41 KB |
// | Gridify_SingleMapper_Generated | 847.8 us | 10.72 us |  9.51 us |  1.02 |    0.02 | 6.8359 | 2.9297 |     43 KB |
// |           Gridify_GlobalMapper | 854.6 us | 15.35 us | 19.42 us |  1.04 |    0.03 | 5.8594 | 2.9297 |     40 KB |
// |               Gridify_NoMapper | 876.5 us | 17.33 us | 28.48 us |  1.07 |    0.04 | 5.8594 | 1.9531 |     44 KB |
// |   Gridify_EachAction_Generated | 877.4 us | 16.82 us | 20.66 us |  1.06 |    0.03 | 7.8125 | 3.9063 |     48 KB |
