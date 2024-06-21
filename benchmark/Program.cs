using System;
using BenchmarkDotNet.Running;

namespace Benchmarks;

public class Program
{
   private static void Main()
   {
      // BenchmarkRunner.Run<LibraryComparisionFilteringBenchmark>();
      BenchmarkRunner.Run<NativeLinqComparisionBenchmark>();
      // BenchmarkRunner.Run<LibraryComparisionFilteringBenchmark2>();
      // BenchmarkRunner.Run<GridifyMapperUsages>();
      // BenchmarkRunner.Run<QueryBuilderBuildBenchmark>();
      // BenchmarkRunner.Run<QueryBuilderEvaluatorBenchmark>();
      Console.Read();
   }
}
