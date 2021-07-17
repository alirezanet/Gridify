using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using BenchmarkDotNet.Running;
using Gridify;
using Gridify.Tests;
using Microsoft.Extensions.Options;
using Sieve.Models;
using Sieve.Services;

namespace Benchmarks
{
   public class Program
   {
      private static void Main()
      {
         BenchmarkRunner.Run<MyClass>();
         Console.Read();
      }

      [MemoryDiagnoser]
      [RPlotExporter]
      [Orderer(SummaryOrderPolicy.FastestToSlowest)]
      public class MyClass
      {
         private IEnumerable<TestClass> _dataSource;

         [GlobalSetup]
         public void Setup()
         {
            _dataSource = GetSampleData().ToList();
         }

         [Benchmark]
         public void Gridify()
         {
            _dataSource.AsQueryable()
               .ApplyFiltering("Name=*a").ToList();
            _dataSource.AsQueryable()
               .ApplyFiltering("Id>>5");
            _dataSource.AsQueryable()
               .ApplyFiltering("Name==Ali");
         }

         [Benchmark(Baseline = true)]
         public void NativeLinQ()
         {
            _dataSource.AsQueryable()
               .Where(q => q.Name.Contains("a")).ToList();
            _dataSource.AsQueryable()
               .Where(q => q.Id > 5);
            _dataSource.AsQueryable()
               .Where(q => q.Name == "Ali");
         }

         [Benchmark]
         public void DynamicLinQ()
         {
            _dataSource.AsQueryable()
               .Where("Name.Contains(@0)", "a").ToList();
            _dataSource.AsQueryable()
               .Where("Id > (@0)", "5");
            _dataSource.AsQueryable()
               .Where("Name==(@0)", "Ali");
         }

         [Benchmark]
         public void Sieve()
         {
            var processor = new SieveProcessor(new OptionsWrapper<SieveOptions>(new SieveOptions()));
            var x = _dataSource.AsQueryable();
            processor.Apply(new SieveModel {Filters = "Name@=a"}, x, applySorting: false, applyPagination: false).ToList();
            processor.Apply(new SieveModel {Filters = "Id>5"}, x, applySorting: false, applyPagination: false).ToList();
            processor.Apply(new SieveModel {Filters = "Name==Ali"}, x, applySorting: false, applyPagination: false).ToList();
         }


         private static IEnumerable<TestClass> GetSampleData()
         {
            var lst = new List<TestClass>();
            lst.Add(new TestClass(1, "John", null, Guid.NewGuid(), DateTime.Now));
            lst.Add(new TestClass(2, "Bob", null, Guid.NewGuid(), DateTime.UtcNow));
            lst.Add(new TestClass(3, "Jack", (TestClass) lst[0].Clone(), Guid.Empty, DateTime.Now.AddDays(2)));
            lst.Add(new TestClass(4, "Rose", null, Guid.Parse("e2cec5dd-208d-4bb5-a852-50008f8ba366")));
            lst.Add(new TestClass(5, "Ali", null));
            lst.Add(new TestClass(6, "Hamid", (TestClass) lst[0].Clone(), Guid.Parse("de12bae1-93fa-40e4-92d1-2e60f95b468c")));
            lst.Add(new TestClass(7, "Hasan", (TestClass) lst[1].Clone()));
            lst.Add(new TestClass(8, "Farhad", (TestClass) lst[2].Clone(), Guid.Empty));
            lst.Add(new TestClass(9, "Sara", null));
            lst.Add(new TestClass(10, "Jorge", null));
            lst.Add(new TestClass(11, "joe", null));
            lst.Add(new TestClass(12, "jimmy", (TestClass) lst[0].Clone()));
            lst.Add(new TestClass(13, "Nazanin", null));
            lst.Add(new TestClass(14, "Reza", null));
            lst.Add(new TestClass(15, "Korosh", (TestClass) lst[0].Clone()));
            lst.Add(new TestClass(16, "Kamran", (TestClass) lst[1].Clone()));
            lst.Add(new TestClass(17, "Saeid", (TestClass) lst[2].Clone()));
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
   }
}