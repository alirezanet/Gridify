using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Gridify.Tests;

/// <summary>
/// Basic tests for CompositeMap feature - tests only what currently works
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
   public void CompositeMap_ExactMatchOnFirstProperty()
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
   public void CompositeMap_ExactMatchOnSecondProperty()
   {
      // Arrange
      var mapper = new GridifyMapper<TestClass>()
          .AddCompositeMap("search", x => x.Name, x => x.Tag);

      var gridifyQuery = new GridifyQuery { Filter = "search=TagB" };

      // Act
      var result = _fakeRepository.AsQueryable().ApplyFiltering(gridifyQuery, mapper).ToList();

      // Assert
      Assert.Single(result);
      Assert.Equal("Bob", result[0].Name);
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
