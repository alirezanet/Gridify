using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Gridify.Tests;

public class CompositeMapTests
{
   private readonly List<TestClass> _fakeRepository;

   public CompositeMapTests()
   {
      _fakeRepository = new List<TestClass>
        {
            new TestClass(1, "John", null) { MyGuid = Guid.Parse("00000000-0000-0000-0000-000000000001"), Tag = "TagA" },
            new TestClass(2, "Bob", null) { MyGuid = Guid.Parse("00000000-0000-0000-0000-000000000002"), Tag = "TagB" },
            new TestClass(3, "Jack", null) { MyGuid = Guid.Parse("00000000-0000-0000-0000-000000000003"), Tag = "TagC" },
            new TestClass(4, "Rose", null) { MyGuid = Guid.Parse("00000000-0000-0000-0000-000000000004"), Tag = "TagD" },
            new TestClass(5, "Ali", null) { MyGuid = Guid.Parse("00000000-0000-0000-0000-000000000005"), Tag = "TagE" },
            new TestClass(6, "Hamid", null) { MyGuid = Guid.Parse("00000000-0000-0000-0000-000000000006"), Tag = "TagF" },
            new TestClass(7, "Hasan", null) { MyGuid = Guid.Parse("00000000-0000-0000-0000-000000000007"), Tag = "TagG" },
            new TestClass(8, "Mohsen", null) { MyGuid = Guid.Parse("00000000-0000-0000-0000-000000000008"), Tag = "TagH" },
            new TestClass(9, "Davide", null) { MyGuid = Guid.Parse("00000000-0000-0000-0000-000000000009"), Tag = "TagI" }
        };
   }

   [Fact]
   public void AddCompositeMap_WithMultipleProperties_ShouldCreateCompositeMapping()
   {
      // Arrange
      var mapper = new GridifyMapper<TestClass>()
          .AddCompositeMap("search", x => x.Name, x => x.Tag);

      // Assert
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
      // Arrange
      var mapper = new GridifyMapper<TestClass>();

      // Act & Assert
      Assert.Throws<GridifyMapperException>(() => mapper.AddCompositeMap("search"));
   }

   [Fact]
   public void CompositeMap_SearchAcrossMultipleProperties_WithEquals()
   {
      // Arrange
      var mapper = new GridifyMapper<TestClass>()
          .AddCompositeMap("search", x => x.Name, x => x.Tag);

      var gridifyQuery = new GridifyQuery { Filter = "search=John" };

      // Act
      var result = _fakeRepository.AsQueryable().ApplyFiltering(gridifyQuery, mapper).ToList();

      // Assert
      Assert.Single(result);
      Assert.Equal("John", result[0].Name);
   }

   [Fact]
   public void CompositeMap_SearchAcrossMultipleProperties_WithContains()
   {
      // Arrange
      var mapper = new GridifyMapper<TestClass>(cfg => cfg.CaseSensitive = false)
          .AddCompositeMap("search", x => x.Name, x => x.Tag);

      var gridifyQuery = new GridifyQuery { Filter = "search=*tag*" };

      // Act
      var result = _fakeRepository.AsQueryable().ApplyFiltering(gridifyQuery, mapper).ToList();

      // Assert - should find all items with Tag property containing "tag" (case-insensitive)
      Assert.Equal(9, result.Count);
   }

   [Fact]
   public void CompositeMap_SearchAcrossMultipleProperties_OrBehavior()
   {
      // Arrange
      var mapper = new GridifyMapper<TestClass>()
          .AddCompositeMap("search", x => x.Name, x => x.Tag);

      var gridifyQuery = new GridifyQuery { Filter = "search=*a*" };

      // Act
      var result = _fakeRepository.AsQueryable().ApplyFiltering(gridifyQuery, mapper).ToList();

      // Assert - should match Name (Jack, Hasan, Davide) OR Tag (TagA, TagB, TagC, etc.)
      // Names containing 'a': Jack, Hasan, Davide, Hamid (4)
      // Tags containing 'a': TagA, TagB, TagC, TagD, TagE, TagF, TagG, TagH, TagI (all 9)
      // All items should match because all have "Tag" prefix in their tags
      Assert.Equal(9, result.Count);
   }

   [Fact]
   public void CompositeMap_WithDifferentTypes_IntAndString()
   {
      // Arrange
      var mapper = new GridifyMapper<TestClass>()
          .AddCompositeMap("search", x => (object)x.Id, x => x.Name);

      var gridifyQuery = new GridifyQuery { Filter = "search=1" };

      // Act
      var result = _fakeRepository.AsQueryable().ApplyFiltering(gridifyQuery, mapper).ToList();

      // Assert - should find Id=1
      Assert.Single(result);
      Assert.Equal(1, result[0].Id);
   }

   [Fact]
   public void CompositeMap_WithStartsWith()
   {
      // Arrange
      var mapper = new GridifyMapper<TestClass>()
          .AddCompositeMap("search", x => x.Name, x => x.Tag);

      var gridifyQuery = new GridifyQuery { Filter = "search=^Tag" };

      // Act
      var result = _fakeRepository.AsQueryable().ApplyFiltering(gridifyQuery, mapper).ToList();

      // Assert - should find all items with Tag starting with "Tag"
      Assert.Equal(9, result.Count);
   }

   [Fact]
   public void CompositeMap_CombinedWithOtherFilters_UsingAnd()
   {
      // Arrange
      var mapper = new GridifyMapper<TestClass>()
          .AddCompositeMap("search", x => x.Name, x => x.Tag)
          .AddMap("id", x => x.Id);

      var gridifyQuery = new GridifyQuery { Filter = "search=*a*, id>5" };

      // Act
      var result = _fakeRepository.AsQueryable().ApplyFiltering(gridifyQuery, mapper).ToList();

      // Assert - should find items with 'a' in Name or Tag AND Id > 5
      Assert.True(result.All(x => x.Id > 5));
      Assert.True(result.Count > 0);
   }

   [Fact]
   public void CompositeMap_CombinedWithOtherFilters_UsingOr()
   {
      // Arrange
      var mapper = new GridifyMapper<TestClass>()
          .AddCompositeMap("search", x => x.Name, x => x.Tag)
          .AddMap("id", x => x.Id);

      var gridifyQuery = new GridifyQuery { Filter = "search=John|id=2" };

      // Act
      var result = _fakeRepository.AsQueryable().ApplyFiltering(gridifyQuery, mapper).ToList();

      // Assert - should find John OR Id=2
      Assert.Equal(2, result.Count);
      Assert.Contains(result, x => x.Name == "John");
      Assert.Contains(result, x => x.Id == 2);
   }

   [Fact]
   public void CompositeMap_WithNestedProperty()
   {
      // Arrange
      var fakeData = new List<TestClass>
        {
            new TestClass(1, "Parent1", new TestClass(10, "Child1", null) { Tag = "ChildTag1" }),
            new TestClass(2, "Parent2", new TestClass(20, "Child2", null) { Tag = "ChildTag2" }),
            new TestClass(3, "Parent3", new TestClass(30, "Child3", null) { Tag = "ChildTag3" })
        };

      var mapper = new GridifyMapper<TestClass>()
          .AddCompositeMap("childsearch", x => x.ChildClass!.Name, x => x.ChildClass!.Tag);

      var gridifyQuery = new GridifyQuery { Filter = "childsearch=*Child2*" };

      // Act
      var result = fakeData.AsQueryable().ApplyFiltering(gridifyQuery, mapper).ToList();

      // Assert - should find Parent2 by matching child name
      Assert.Single(result);
      Assert.Equal("Parent2", result[0].Name);
   }

   [Fact]
   public void CompositeMap_WithThreeProperties()
   {
      // Arrange
      var mapper = new GridifyMapper<TestClass>()
          .AddCompositeMap("search", x => x.Name, x => x.Tag, x => (object)x.Id);

      var gridifyQuery = new GridifyQuery { Filter = "search=5" };

      // Act
      var result = _fakeRepository.AsQueryable().ApplyFiltering(gridifyQuery, mapper).ToList();

      // Assert - should find Id=5
      Assert.Single(result);
      Assert.Equal(5, result[0].Id);
   }

   [Fact]
   public void CompositeMap_CaseSensitivity_Default()
   {
      // Arrange
      var mapper = new GridifyMapper<TestClass>(q => q.CaseSensitive = false)
          .AddCompositeMap("search", x => x.Name, x => x.Tag);

      var gridifyQuery = new GridifyQuery { Filter = "search=john" };

      // Act
      var result = _fakeRepository.AsQueryable().ApplyFiltering(gridifyQuery, mapper).ToList();

      // Assert - should find "John" with case-insensitive search
      Assert.Single(result);
      Assert.Equal("John", result[0].Name);
   }

   [Fact]
   public void CompositeMap_WithNotEquals()
   {
      // Arrange
      var mapper = new GridifyMapper<TestClass>()
          .AddCompositeMap("search", x => x.Name, x => x.Tag);

      var gridifyQuery = new GridifyQuery { Filter = "search!=John" };

      // Act
      var result = _fakeRepository.AsQueryable().ApplyFiltering(gridifyQuery, mapper).ToList();

      // Assert - should find all items except the one with Name=John AND Tag=John (which doesn't exist)
      // Since only Name=John exists, all items where Name!=John OR Tag!=John should match
      Assert.Equal(8, result.Count);
      Assert.DoesNotContain(result, x => x.Name == "John");
   }

   [Fact]
   public void CompositeMap_CanBeOverridden()
   {
      // Arrange
      var mapper = new GridifyMapper<TestClass>()
          .AddCompositeMap("search", x => x.Name, x => x.Tag)
          .AddCompositeMap("search", x => x.Name); // Override with single property

      var gridifyQuery = new GridifyQuery { Filter = "search=TagA" };

      // Act
      var result = _fakeRepository.AsQueryable().ApplyFiltering(gridifyQuery, mapper).ToList();

      // Assert - should not find anything because we're only searching Name now, not Tag
      Assert.Empty(result);
   }

   [Fact]
   public void CompositeMap_WithGuidType()
   {
      // Arrange
      var mapper = new GridifyMapper<TestClass>()
          .AddCompositeMap("search", x => (object)x.MyGuid, x => x.Name);

      var gridifyQuery = new GridifyQuery { Filter = "search=00000000-0000-0000-0000-000000000001" };

      // Act
      var result = _fakeRepository.AsQueryable().ApplyFiltering(gridifyQuery, mapper).ToList();

      // Assert - should find by GUID
      Assert.Single(result);
      Assert.Equal(1, result[0].Id);
   }
}
