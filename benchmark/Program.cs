using System;
using BenchmarkDotNet.Running;

namespace Benchmarks
{
   public class Program
   {
      private static void Main()
      {
         BenchmarkRunner.Run<LibraryComparisionFilteringBenchmark>();
         // BenchmarkRunner.Run<QueryBuilderBuildBenchmark>();
         // BenchmarkRunner.Run<QueryBuilderEvaluatorBenchmark>();
         Console.Read();
      }
   }
}
