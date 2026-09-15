using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Gridify.Tests.IssueTests;

public class CaseInsensitivePerMapTests
{
   private List<TestClass> DataSource => new()
   {
      new TestClass { Name = "Alice", Tags = ["CSharp", "DotNet"] },
      new TestClass { Name = "Bob",   Tags = ["Python", "Django"] },
   };

   // --- string field: caseInsensitive=false overrides global CaseInsensitiveFiltering=true ---

   [Fact]
   public void AddMap_CaseSensitiveTrue_OnStringField_ShouldNotMatchWrongCase()
   {
      var mapper = new GridifyMapper<TestClass>(c => c.CaseInsensitiveFiltering = true)
         .AddMap("Name", x => x.Name, caseInsensitive: false);

      Assert.Empty(DataSource.AsQueryable().ApplyFiltering("Name=alice", mapper).ToList());
   }

   [Fact]
   public void AddMap_CaseSensitiveTrue_OnStringField_ShouldMatchCorrectCase()
   {
      var mapper = new GridifyMapper<TestClass>(c => c.CaseInsensitiveFiltering = true)
         .AddMap("Name", x => x.Name, caseInsensitive: false);

      Assert.Single(DataSource.AsQueryable().ApplyFiltering("Name=Alice", mapper).ToList());
   }

   // --- string field: caseInsensitive=true overrides global CaseInsensitiveFiltering=false ---

   [Fact]
   public void AddMap_CaseSensitiveFalse_OnStringField_ShouldMatchWrongCaseWhenGlobalIsOff()
   {
      var mapper = new GridifyMapper<TestClass>(c => c.CaseInsensitiveFiltering = false)
         .AddMap("Name", x => x.Name, caseInsensitive: true);

      Assert.Single(DataSource.AsQueryable().ApplyFiltering("Name=alice", mapper).ToList());
   }

   // --- List<string> field: use the (string, convertor, caseInsensitive) overload so CreateExpression
   //     auto-wraps the collection with .Select(fc => fc), making IsNestedCollection() true ---

   [Fact]
   public void AddMap_CaseSensitiveTrue_OnListOfStrings_ShouldNotMatchWrongCase()
   {
      var mapper = new GridifyMapper<TestClass>(c => c.CaseInsensitiveFiltering = true)
         .AddMap("Tags", caseInsensitive: false);

      Assert.Empty(DataSource.AsQueryable().ApplyFiltering("Tags=csharp", mapper).ToList());
   }

   [Fact]
   public void AddMap_CaseSensitiveTrue_OnListOfStrings_ShouldMatchCorrectCase()
   {
      var mapper = new GridifyMapper<TestClass>(c => c.CaseInsensitiveFiltering = true)
         .AddMap("Tags", caseInsensitive: false);

      Assert.Single(DataSource.AsQueryable().ApplyFiltering("Tags=CSharp", mapper).ToList());
   }

   [Fact]
   public void AddMap_CaseSensitiveFalse_OnListOfStrings_ShouldMatchWrongCaseWhenGlobalIsOff()
   {
      var mapper = new GridifyMapper<TestClass>(c => c.CaseInsensitiveFiltering = false)
         .AddMap("Tags", caseInsensitive: true);

      Assert.Single(DataSource.AsQueryable().ApplyFiltering("Tags=csharp", mapper).ToList());
   }

   private class TestClass
   {
      public string Name { get; set; } = "";
      public List<string> Tags { get; set; } = [];
   }
}
