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
public class NativeLinqComparisonBenchmark
{
   private static readonly Consumer Consumer = new();
   private TestClass[] _data;
   private GridifyMapper<TestClass> _gm;

   private IQueryable<TestClass> Ds => _data.AsQueryable();

   [GlobalSetup]
   public void Setup()
   {
      _data = LibraryComparisionFilteringBenchmark.GetSampleData().ToArray();
      _gm = new GridifyMapper<TestClass>(true);
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
}
