using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Gridify.Tests.IssueTests;

public class Issue191Tests
{
   private readonly List<TestClass> _fakeRepository = [.. GridifyExtensionsShould.GetSampleData()];

   [Fact]
   public void ApplyFiltering_Equals_GlobalCaseInsensitiveSearch()
   {
      var mapper = new GridifyMapper<TestClass>(m => m.CaseInsensitiveFiltering = true).GenerateMappings();

      var gq = new GridifyQuery { Filter = "name=BOB" };

      var expected = _fakeRepository.Where(q => string.Equals(q.Name!, "BOB", StringComparison.InvariantCultureIgnoreCase)).ToList();

      var actual = _fakeRepository.AsQueryable()
          .ApplyFiltering(gq, mapper)
          .ToList();

      Assert.Equal(expected.Count, actual.Count);
      Assert.Equal(expected, actual);
      Assert.True(actual.Any());
   }

   [Fact]
   public void ApplyFiltering_NotEquals_GlobalCaseInsensitiveSearch()
   {
      var mapper = new GridifyMapper<TestClass>(m => m.CaseInsensitiveFiltering = true).GenerateMappings();

      var gq = new GridifyQuery { Filter = "name!=BOB" };

      var expected = _fakeRepository.Where(q => !string.Equals(q.Name!, "BOB", StringComparison.InvariantCultureIgnoreCase)).ToList();

      var actual = _fakeRepository.AsQueryable()
         .ApplyFiltering(gq, mapper)
         .ToList();

      Assert.Equal(expected.Count, actual.Count);
      Assert.Equal(expected, actual);
      Assert.True(actual.Any());
   }

   [Fact]
   public void ApplyFiltering_Contains_GlobalCaseInsensitiveSearch()
   {
      var mapper = new GridifyMapper<TestClass>(m => m.CaseInsensitiveFiltering = true).GenerateMappings();

      var gq = new GridifyQuery { Filter = "name=*BO" };

      var expected = _fakeRepository.Where(q => q.Name!.Contains("BO", StringComparison.InvariantCultureIgnoreCase)).ToList();

      var actual = _fakeRepository.AsQueryable()
         .ApplyFiltering(gq, mapper)
         .ToList();

      Assert.Equal(expected.Count, actual.Count);
      Assert.Equal(expected, actual);
      Assert.True(actual.Any());
   }

   [Fact]
   public void ApplyFiltering_NotContains_GlobalCaseInsensitiveSearch()
   {
      var mapper = new GridifyMapper<TestClass>(m => m.CaseInsensitiveFiltering = true).GenerateMappings();

      var gq = new GridifyQuery { Filter = "name!*BO" };

      var expected = _fakeRepository.Where(q => !q.Name!.Contains("BO", StringComparison.InvariantCultureIgnoreCase)).ToList();

      var actual = _fakeRepository.AsQueryable()
         .ApplyFiltering(gq, mapper)
         .ToList();

      Assert.Equal(expected.Count, actual.Count);
      Assert.Equal(expected, actual);
      Assert.True(actual.Any());
   }

   [Fact]
   public void ApplyFiltering_StartWith_GlobalCaseInsensitiveSearch()
   {
      var mapper = new GridifyMapper<TestClass>(m => m.CaseInsensitiveFiltering = true).GenerateMappings();

      var gq = new GridifyQuery { Filter = "name^BO" };

      var expected = _fakeRepository.Where(q => q.Name!.StartsWith("BO", StringComparison.InvariantCultureIgnoreCase)).ToList();

      var actual = _fakeRepository.AsQueryable()
         .ApplyFiltering(gq, mapper)
         .ToList();

      Assert.Equal(expected.Count, actual.Count);
      Assert.Equal(expected, actual);
      Assert.True(actual.Any());
   }

   [Fact]
   public void ApplyFiltering_NotStartWith_GlobalCaseInsensitiveSearch()
   {
      var mapper = new GridifyMapper<TestClass>(m => m.CaseInsensitiveFiltering = true).GenerateMappings();

      var gq = new GridifyQuery { Filter = "name!^BO" };

      var expected = _fakeRepository.Where(q => !q.Name!.StartsWith("BO", StringComparison.InvariantCultureIgnoreCase)).ToList();

      var actual = _fakeRepository.AsQueryable()
         .ApplyFiltering(gq, mapper)
         .ToList();

      Assert.Equal(expected.Count, actual.Count);
      Assert.Equal(expected, actual);
      Assert.True(actual.Any());
   }

   [Fact]
   public void ApplyFiltering_EndWith_GlobalCaseInsensitiveSearch()
   {
      var mapper = new GridifyMapper<TestClass>(m => m.CaseInsensitiveFiltering = true).GenerateMappings();

      var gq = new GridifyQuery { Filter = "name$OB" };

      var expected = _fakeRepository.Where(q => q.Name!.EndsWith("OB", StringComparison.InvariantCultureIgnoreCase)).ToList();

      var actual = _fakeRepository.AsQueryable()
         .ApplyFiltering(gq, mapper)
         .ToList();

      Assert.Equal(expected.Count, actual.Count);
      Assert.Equal(expected, actual);
      Assert.True(actual.Any());
   }

   [Fact]
   public void ApplyFiltering_NotEndWith_GlobalCaseInsensitiveSearch()
   {
      var mapper = new GridifyMapper<TestClass>(m => m.CaseInsensitiveFiltering = true).GenerateMappings();

      var gq = new GridifyQuery { Filter = "name!$OB" };

      var expected = _fakeRepository.Where(q => !q.Name!.EndsWith("OB", StringComparison.InvariantCultureIgnoreCase)).ToList();

      var actual = _fakeRepository.AsQueryable()
         .ApplyFiltering(gq, mapper)
         .ToList();

      Assert.Equal(expected.Count, actual.Count);
      Assert.Equal(expected, actual);
      Assert.True(actual.Any());
   }

}
