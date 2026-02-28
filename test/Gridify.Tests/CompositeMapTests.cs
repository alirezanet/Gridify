using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Gridify.Tests;

/// <summary>
/// Comprehensive tests for CompositeMap feature covering all working operators
/// </summary>
public class CompositeMapBasicTests
{
   private readonly List<TestClass> _fakeRepository;

   public CompositeMapBasicTests()
   {
      _fakeRepository = new List<TestClass>
        {
            new TestClass(1, "John", null) { MyGuid = Guid.Parse("00000000-0000-0000-0000-000000000001"), Tag = "TagA" },
            new TestClass(2, "Bob", null) { MyGuid = Guid.Parse("00000000-0000-0000-0000-000000000002"), Tag = "TagB" },
            new TestClass(3, "Jack", null) { MyGuid = Guid.Parse("00000000-0000-0000-0000-000000000003"), Tag = "TagC" },
            new TestClass(4, "Rose", null) { MyGuid = Guid.Parse("00000000-0000-0000-0000-000000000004"), Tag = "TagD" },
            new TestClass(5, "Ali", null) { MyGuid = Guid.Parse("00000000-0000-0000-0000-000000000005"), Tag = "TagE" }
        };
   }

   [Fact]
   public void AddCompositeMap_WithMultipleProperties_ShouldCreateCompositeMapping()
   {
      var mapper = new GridifyMapper<TestClass>()
          .AddCompositeMap("search", x => x.Name, x => x.Tag);
      Assert.True(mapper.HasMap("search"));
      var gMap = mapper.GetGMap("search");
      Assert.NotNull(gMap);
      Assert.IsType<CompositeGMap<TestClass>>(gMap);
      var compositeMap = (CompositeGMap<TestClass>)gMap;
      Assert.Equal(2, compositeMap.Expressions.Count);
      Assert.True(compositeMap.IsComposite);
   }

   [Fact]
   public void AddCompositeMap_WithEmptyExpressions_ShouldThrowException()
   {
      var mapper = new GridifyMapper<TestClass>();
      Assert.Throws<GridifyMapperException>(() => mapper.AddCompositeMap("search"));
   }

   [Fact]
   public void CompositeMap_Equal_Operator()
   {
      var mapper = new GridifyMapper<TestClass>()
          .AddCompositeMap("search", x => x.Name, x => x.Tag);
      var result = _fakeRepository.AsQueryable().ApplyFiltering("search=John", mapper).ToList();
      Assert.Single(result);
      Assert.Equal("John", result[0].Name);
   }

   [Fact]
   public void CompositeMap_Equal_MatchesSecondProperty()
   {
      var mapper = new GridifyMapper<TestClass>()
          .AddCompositeMap("search", x => x.Name, x => x.Tag);
      var result = _fakeRepository.AsQueryable().ApplyFiltering("search=TagB", mapper).ToList();
      Assert.Single(result);
      Assert.Equal("Bob", result[0].Name);
   }

   [Fact]
   public void CompositeMap_StartsWith_Operator()
   {
      var mapper = new GridifyMapper<TestClass>(cfg => cfg.CaseInsensitiveFiltering = true)
          .AddCompositeMap("search", x => x.Name, x => x.Tag);
      var result = _fakeRepository.AsQueryable().ApplyFiltering("search^Tag", mapper).ToList();
      Assert.Equal(5, result.Count);
   }

   [Fact]
   public void CompositeMap_EndsWith_Operator()
   {
      var mapper = new GridifyMapper<TestClass>(cfg => cfg.CaseInsensitiveFiltering = true)
          .AddCompositeMap("search", x => x.Name, x => x.Tag);
      var result = _fakeRepository.AsQueryable().ApplyFiltering("search$hn", mapper).ToList();
      Assert.Single(result);
      Assert.Equal("John", result[0].Name);
   }

   [Fact]
   public void CompositeMap_WithConvertor()
   {
      var mapper = new GridifyMapper<TestClass>()
          .AddCompositeMap("search", val => val.ToUpper(), x => x.Name, x => x.Tag);
      var result = _fakeRepository.AsQueryable().ApplyFiltering("search=john", mapper).ToList();
      Assert.Empty(result);
   }

   [Fact]
   public void CompositeMap_WithConvertor_MatchesAfterConversion()
   {
      // Convertor prepends "Tag" to the search value
      Func<string, object> convertor = val => "Tag" + val;
      var mapper = new GridifyMapper<TestClass>()
          .AddCompositeMap("search", convertor, x => x.Tag);
      var result = _fakeRepository.AsQueryable().ApplyFiltering("search=A", mapper).ToList();
      Assert.Single(result);
      Assert.Equal("TagA", result[0].Tag);
   }

   [Fact]
   public void CompositeMap_GreaterThan_OnNumericProperty()
   {
      var mapper = new GridifyMapper<TestClass>()
          .AddCompositeMap("search", x => (object)x.Id);
      var result = _fakeRepository.AsQueryable().ApplyFiltering("search>3", mapper).ToList();
      Assert.Equal(2, result.Count);
      Assert.True(result.All(x => x.Id > 3));
   }

   [Fact]
   public void CompositeMap_LessThan_OnNumericProperty()
   {
      var mapper = new GridifyMapper<TestClass>()
          .AddCompositeMap("search", x => (object)x.Id);
      var result = _fakeRepository.AsQueryable().ApplyFiltering("search<3", mapper).ToList();
      Assert.Equal(2, result.Count);
      Assert.True(result.All(x => x.Id < 3));
   }

   [Fact]
   public void CompositeMap_GreaterOrEqual_OnNumericProperty()
   {
      var mapper = new GridifyMapper<TestClass>()
          .AddCompositeMap("search", x => (object)x.Id);
      var result = _fakeRepository.AsQueryable().ApplyFiltering("search>=4", mapper).ToList();
      Assert.Equal(2, result.Count);
      Assert.True(result.All(x => x.Id >= 4));
   }

   [Fact]
   public void CompositeMap_LessOrEqual_OnNumericProperty()
   {
      var mapper = new GridifyMapper<TestClass>()
          .AddCompositeMap("search", x => (object)x.Id);
      var result = _fakeRepository.AsQueryable().ApplyFiltering("search<=2", mapper).ToList();
      Assert.Equal(2, result.Count);
      Assert.True(result.All(x => x.Id <= 2));
   }

   [Fact]
   public void CompositeMap_CombinedWithOtherFilters_UsingOr()
   {
      var mapper = new GridifyMapper<TestClass>()
          .AddCompositeMap("search", x => x.Name, x => x.Tag)
          .AddMap("id", x => x.Id);
      var result = _fakeRepository.AsQueryable().ApplyFiltering("search=John|id=2", mapper).ToList();
      Assert.Equal(2, result.Count);
      Assert.Contains(result, x => x.Name == "John");
      Assert.Contains(result, x => x.Id == 2);
   }

   [Fact]
   public void CompositeMap_CombinedWithOtherFilters_UsingAnd()
   {
      var mapper = new GridifyMapper<TestClass>()
          .AddCompositeMap("search", x => x.Name, x => x.Tag)
          .AddMap("id", x => x.Id);
      var result = _fakeRepository.AsQueryable().ApplyFiltering("search=John,id=1", mapper).ToList();
      Assert.Single(result);
      Assert.Equal("John", result[0].Name);
      Assert.Equal(1, result[0].Id);
   }

   [Fact]
   public void CompositeMap_WithThreeProperties()
   {
      var mapper = new GridifyMapper<TestClass>()
          .AddCompositeMap("search", x => x.Name, x => x.Tag, x => (object)x.Id);
      var result = _fakeRepository.AsQueryable().ApplyFiltering("search=5", mapper).ToList();
      Assert.Single(result);
      Assert.Equal(5, result[0].Id);
   }

   [Fact]
   public void CompositeMap_WithGuidType()
   {
      var mapper = new GridifyMapper<TestClass>()
          .AddCompositeMap("search", x => (object)x.MyGuid, x => x.Name);
      var result = _fakeRepository.AsQueryable().ApplyFiltering("search=00000000-0000-0000-0000-000000000001", mapper).ToList();
      Assert.Single(result);
      Assert.Equal(1, result[0].Id);
   }

   [Fact]
   public void CompositeMap_WithMixedStringTypes()
   {
      var mapper = new GridifyMapper<TestClass>()
          .AddCompositeMap("search", x => x.Name, x => x.Tag);
      var result = _fakeRepository.AsQueryable().ApplyFiltering("search=Bob", mapper).ToList();
      Assert.Single(result);
      Assert.Equal("Bob", result[0].Name);
   }

   [Fact]
   public void CompositeMap_ParenthesizedExpression()
   {
      var mapper = new GridifyMapper<TestClass>()
          .AddCompositeMap("search", x => x.Name, x => x.Tag)
          .AddMap("id", x => x.Id);
      var result = _fakeRepository.AsQueryable().ApplyFiltering("(search=John|id=2),id<3", mapper).ToList();
      Assert.True(result.Count > 0);
      Assert.True(result.All(x => x.Id < 3));
   }

   [Fact]
   public void CompositeMap_CanBeOverridden()
   {
      var mapper = new GridifyMapper<TestClass>()
          .AddCompositeMap("search", x => x.Name, x => x.Tag)
          .AddCompositeMap("search", x => x.Name); // Override
      var result = _fakeRepository.AsQueryable().ApplyFiltering("search=TagA", mapper).ToList();
      Assert.Empty(result); // Only searches Name now, not Tag
   }

   [Fact]
   public void CompositeMap_LikeOperator_Contains()
   {
      var mapper = new GridifyMapper<TestClass>()
          .AddCompositeMap("search", x => x.Name, x => x.Tag);
      var result = _fakeRepository.AsQueryable().ApplyFiltering("search=*oh", mapper).ToList();
      Assert.Single(result); // John contains "oh"
      Assert.Equal("John", result[0].Name);
   }
}
