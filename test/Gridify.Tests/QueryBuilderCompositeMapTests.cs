using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Gridify.Tests;

/// <summary>
/// Tests for AddCompositeMap feature in QueryBuilder
/// </summary>
public class QueryBuilderCompositeMapTests
{
   private readonly List<TestClass> _fakeRepository;

   public QueryBuilderCompositeMapTests()
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
   public void AddCompositeMap_BasicUsage_ShouldWorkWithBuild()
   {
      // Arrange
      var builder = new QueryBuilder<TestClass>()
         .AddCompositeMap("search", x => x.Name, x => x.Tag)
         .AddCondition("search=John");

      // Act
      var result = builder.Build(_fakeRepository.AsQueryable()).ToList();

      // Assert
      Assert.Single(result);
      Assert.Equal("John", result[0].Name);
   }

   [Fact]
   public void AddCompositeMap_BasicUsage_ShouldWorkWithBuildIEnumerable()
   {
      // Arrange
      var builder = new QueryBuilder<TestClass>()
         .AddCompositeMap("search", x => x.Name, x => x.Tag)
         .AddCondition("search=TagB");

      // Act
      var result = builder.Build(_fakeRepository).ToList();

      // Assert
      Assert.Single(result);
      Assert.Equal("Bob", result[0].Name);
   }

   [Fact]
   public void AddCompositeMap_WithMultipleProperties_ShouldMatchEither()
   {
      // Arrange
      var builder = new QueryBuilder<TestClass>()
         .AddCompositeMap("search", x => x.Name, x => x.Tag);

      // Act - Search for "Jack" which is in Name
      var result1 = builder
         .AddCondition("search=Jack")
         .Build(_fakeRepository.AsQueryable())
         .ToList();

      // Search for "TagA" which is in Tag
      var builder2 = new QueryBuilder<TestClass>()
         .AddCompositeMap("search", x => x.Name, x => x.Tag)
         .AddCondition("search=TagA");
      var result2 = builder2.Build(_fakeRepository.AsQueryable()).ToList();

      // Assert
      Assert.Single(result1);
      Assert.Equal("Jack", result1[0].Name);
      Assert.Single(result2);
      Assert.Equal("John", result2[0].Name);
   }

   [Fact]
   public void AddCompositeMap_WithConvertor_ShouldApplyConversion()
   {
      // Arrange
      var builder = new QueryBuilder<TestClass>()
         .AddCompositeMap("search", val => val.ToUpper(), x => x.Name, x => x.Tag)
         .AddCondition("search=john");

      // Act
      var result = builder.Build(_fakeRepository.AsQueryable()).ToList();

      // Assert - Should not match because convertor makes it uppercase
      Assert.Empty(result);
   }

   [Fact]
   public void AddCompositeMap_WithConvertor_MatchesAfterConversion()
   {
      // Arrange
      Func<string, object> convertor = val => "Tag" + val;
      var builder = new QueryBuilder<TestClass>()
         .AddCompositeMap("search", convertor, x => x.Tag)
         .AddCondition("search=A");

      // Act
      var result = builder.Build(_fakeRepository.AsQueryable()).ToList();

      // Assert
      Assert.Single(result);
      Assert.Equal("TagA", result[0].Tag);
   }

   [Fact]
   public void AddCompositeMap_CombinedWithNormalMap_ShouldWork()
   {
      // Arrange
      var builder = new QueryBuilder<TestClass>()
         .AddCompositeMap("search", x => x.Name, x => x.Tag)
         .AddMap("id", x => x.Id)
         .AddCondition("search=John")
         .AddCondition("id=1");

      // Act
      var result = builder.Build(_fakeRepository.AsQueryable()).ToList();

      // Assert
      Assert.Single(result);
      Assert.Equal("John", result[0].Name);
      Assert.Equal(1, result[0].Id);
   }

   [Fact]
   public void AddCompositeMap_WithOrdering_ShouldWork()
   {
      // Arrange
      var builder = new QueryBuilder<TestClass>()
         .AddCompositeMap("search", x => (object)x.Id)
         .AddCondition("search>2")
         .AddMap("name", x => x.Name)
         .AddOrderBy("name desc");

      // Act
      var result = builder.Build(_fakeRepository.AsQueryable()).ToList();

      // Assert
      Assert.Equal(3, result.Count);
      Assert.True(result.All(x => x.Id > 2));
      // Check ordering
      Assert.Equal("Rose", result[0].Name); // R comes after J and A in descending order
   }

   [Fact]
   public void AddCompositeMap_WithPaging_ShouldWork()
   {
      // Arrange
      var builder = new QueryBuilder<TestClass>()
         .AddCompositeMap("search", x => (object)x.Id)
         .AddCondition("search>0")
         .ConfigurePaging(1, 2);

      // Act
      var result = builder.Build(_fakeRepository.AsQueryable()).ToList();

      // Assert
      Assert.Equal(2, result.Count); // Page size is 2
      Assert.Equal(3, result[0].Id); // Second page, so starts from Id 3
   }

   [Fact]
   public void AddCompositeMap_BuildWithPaging_ShouldReturnCorrectCount()
   {
      // Arrange
      var builder = new QueryBuilder<TestClass>()
         .AddCompositeMap("search", x => (object)x.Id)
         .AddCondition("search>2")
         .ConfigurePaging(0, 2);

      // Act
      var (totalCount, result) = builder.BuildWithPaging(_fakeRepository.AsQueryable());

      // Assert
      Assert.Equal(3, totalCount); // 3 items match the filter
      Assert.Equal(2, result.Count()); // But only 2 in the first page
   }

   [Fact]
   public void AddCompositeMap_BuildWithPagingIEnumerable_ShouldReturnCorrectCount()
   {
      // Arrange
      var builder = new QueryBuilder<TestClass>()
         .AddCompositeMap("search", x => x.Name, x => x.Tag)
         .AddCondition("search=*a") // Match names or tags containing 'a'
         .ConfigurePaging(0, 3);

      // Act
      var (totalCount, result) = builder.BuildWithPaging(_fakeRepository);

      // Assert
      Assert.True(totalCount >= 3);
      Assert.True(result.Count() <= 3);
   }

   [Fact]
   public void AddCompositeMap_BuildCompiledEvaluator_ShouldWork()
   {
      // Arrange
      var builder = new QueryBuilder<TestClass>()
         .AddCompositeMap("search", x => x.Name, x => x.Tag)
         .AddCondition("search=John");

      // Act
      var evaluator = builder.BuildCompiledEvaluator();
      var isValid = evaluator(_fakeRepository);

      // Assert
      Assert.True(isValid);
   }

   [Fact]
   public void AddCompositeMap_BuildEvaluator_ShouldWork()
   {
      // Arrange
      var builder = new QueryBuilder<TestClass>()
         .AddCompositeMap("search", x => x.Name, x => x.Tag)
         .AddCondition("search=TagC");

      // Act
      var evaluator = builder.BuildEvaluator();
      var isValid = evaluator(_fakeRepository.AsQueryable());

      // Assert
      Assert.True(isValid);
   }

   [Fact]
   public void AddCompositeMap_Evaluate_IQueryable_ShouldWork()
   {
      // Arrange
      var builder = new QueryBuilder<TestClass>()
         .AddCompositeMap("search", x => x.Name, x => x.Tag)
         .AddCondition("search=Rose");

      // Act
      var isValid = builder.Evaluate(_fakeRepository.AsQueryable());

      // Assert
      Assert.True(isValid);
   }

   [Fact]
   public void AddCompositeMap_Evaluate_IEnumerable_ShouldWork()
   {
      // Arrange
      var builder = new QueryBuilder<TestClass>()
         .AddCompositeMap("search", x => x.Name, x => x.Tag)
         .AddCondition("search=Ali");

      // Act
      var isValid = builder.Evaluate(_fakeRepository);

      // Assert
      Assert.True(isValid);
   }

   [Fact]
   public void AddCompositeMap_BuildCompiled_ShouldWork()
   {
      // Arrange
      var builder = new QueryBuilder<TestClass>()
         .AddCompositeMap("search", x => x.Name, x => x.Tag)
         .AddCondition("search=Bob");

      // Act
      var compiled = builder.BuildCompiled();
      var result = compiled(_fakeRepository).ToList();

      // Assert
      Assert.Single(result);
      Assert.Equal("Bob", result[0].Name);
   }

   [Fact]
   public void AddCompositeMap_BuildWithPagingCompiled_ShouldWork()
   {
      // Arrange
      var builder = new QueryBuilder<TestClass>()
         .AddCompositeMap("search", x => (object)x.Id)
         .AddCondition("search<=3")
         .ConfigurePaging(0, 2);

      // Act
      var func = builder.BuildWithPagingCompiled();
      var (totalCount, result) = func(_fakeRepository);

      // Assert
      Assert.Equal(3, totalCount);
      Assert.Equal(2, result.Count());
   }

   [Fact]
   public void AddCompositeMap_IsValid_ShouldReturnTrue_ForValidFilter()
   {
      // Arrange
      var builder = new QueryBuilder<TestClass>()
         .AddCompositeMap("search", x => x.Name, x => x.Tag)
         .AddCondition("search=John");

      // Act
      var isValid = builder.IsValid();

      // Assert
      Assert.True(isValid);
   }

   [Fact]
   public void AddCompositeMap_IsValid_ShouldReturnFalse_ForInvalidMapping()
   {
      // Arrange
      var builder = new QueryBuilder<TestClass>()
         .AddCompositeMap("search", x => x.Name, x => x.Tag)
         .AddCondition("nonExistent=John");

      // Act
      var isValid = builder.IsValid();

      // Assert
      Assert.False(isValid);
   }

   [Fact]
   public void AddCompositeMap_WithThreeProperties_ShouldMatchAny()
   {
      // Arrange
      var builder = new QueryBuilder<TestClass>()
         .AddCompositeMap("search", x => x.Name, x => x.Tag, x => (object)x.Id)
         .AddCondition("search=5");

      // Act
      var result = builder.Build(_fakeRepository.AsQueryable()).ToList();

      // Assert
      Assert.Single(result);
      Assert.Equal(5, result[0].Id);
   }

   [Fact]
   public void AddCompositeMap_WithGreaterThanOperator_ShouldWork()
   {
      // Arrange
      var builder = new QueryBuilder<TestClass>()
         .AddCompositeMap("search", x => (object)x.Id)
         .AddCondition("search>3");

      // Act
      var result = builder.Build(_fakeRepository.AsQueryable()).ToList();

      // Assert
      Assert.Equal(2, result.Count);
      Assert.True(result.All(x => x.Id > 3));
   }

   [Fact]
   public void AddCompositeMap_WithLikeOperator_ShouldWork()
   {
      // Arrange
      var builder = new QueryBuilder<TestClass>()
         .AddCompositeMap("search", x => x.Name, x => x.Tag)
         .AddCondition("search=*oh");

      // Act
      var result = builder.Build(_fakeRepository.AsQueryable()).ToList();

      // Assert
      Assert.Single(result);
      Assert.Equal("John", result[0].Name);
   }

   [Fact]
   public void AddCompositeMap_WithStartsWithOperator_ShouldWork()
   {
      // Arrange
      var builder = new QueryBuilder<TestClass>()
         .AddCompositeMap("search", x => x.Name, x => x.Tag)
         .AddCondition("search^Tag");

      // Act
      var result = builder.Build(_fakeRepository.AsQueryable()).ToList();

      // Assert
      Assert.Equal(5, result.Count); // All tags start with "Tag"
   }

   [Fact]
   public void AddCompositeMap_WithEndsWithOperator_ShouldWork()
   {
      // Arrange
      var builder = new QueryBuilder<TestClass>()
         .AddCompositeMap("search", x => x.Name, x => x.Tag)
         .AddCondition("search$ck");

      // Act
      var result = builder.Build(_fakeRepository.AsQueryable()).ToList();

      // Assert
      Assert.Single(result);
      Assert.Equal("Jack", result[0].Name);
   }

   [Fact]
   public void AddCompositeMap_MultipleConditions_ShouldCombineWithAnd()
   {
      // Arrange
      var builder = new QueryBuilder<TestClass>()
         .AddCompositeMap("search", x => x.Name, x => x.Tag)
         .AddCondition("search^J") // Starts with J
         .AddCondition("search$hn"); // Ends with hn

      // Act
      var result = builder.Build(_fakeRepository.AsQueryable()).ToList();

      // Assert
      Assert.Single(result);
      Assert.Equal("John", result[0].Name);
   }

   [Fact]
   public void AddCompositeMap_BuildFilteringExpression_ShouldCreateValidExpression()
   {
      // Arrange
      var builder = new QueryBuilder<TestClass>()
         .AddCompositeMap("search", x => x.Name, x => x.Tag)
         .AddCondition("search=John");

      // Act
      var expression = builder.BuildFilteringExpression();
      var result = _fakeRepository.AsQueryable().Where(expression).ToList();

      // Assert
      Assert.Single(result);
      Assert.Equal("John", result[0].Name);
   }

   [Fact]
   public void AddCompositeMap_BuildWithQueryablePaging_ShouldWork()
   {
      // Arrange
      var builder = new QueryBuilder<TestClass>()
         .AddCompositeMap("search", x => (object)x.Id)
         .AddCondition("search>1")
         .ConfigurePaging(0, 3);

      // Act
      var (totalCount, query) = builder.BuildWithQueryablePaging(_fakeRepository.AsQueryable());
      var result = query.ToList();

      // Assert
      Assert.Equal(4, totalCount);
      Assert.Equal(3, result.Count);
   }

   [Fact]
   public void AddCompositeMap_CanBeOverriddenWithAnotherCompositeMap()
   {
      // Arrange
      var builder = new QueryBuilder<TestClass>()
         .AddCompositeMap("search", x => x.Name, x => x.Tag)
         .AddCompositeMap("search", x => x.Name) // Override
         .AddCondition("search=TagA");

      // Act
      var result = builder.Build(_fakeRepository.AsQueryable()).ToList();

      // Assert
      Assert.Empty(result); // Should only search Name now, not Tag
   }

   [Fact]
   public void AddCompositeMap_WithGuidType_ShouldWork()
   {
      // Arrange
      var builder = new QueryBuilder<TestClass>()
         .AddCompositeMap("search", x => (object)x.MyGuid, x => x.Name)
         .AddCondition("search=00000000-0000-0000-0000-000000000001");

      // Act
      var result = builder.Build(_fakeRepository.AsQueryable()).ToList();

      // Assert
      Assert.Single(result);
      Assert.Equal(1, result[0].Id);
   }

   [Fact]
   public void AddCompositeMap_WithAddQuery_ShouldWork()
   {
      // Arrange
      var gridifyQuery = new GridifyQuery
      {
         Filter = "search=Bob",
         OrderBy = "id desc",
         Page = 0,
         PageSize = 10
      };

      var builder = new QueryBuilder<TestClass>()
         .AddCompositeMap("search", x => x.Name, x => x.Tag)
         .AddQuery(gridifyQuery);

      // Act
      var result = builder.Build(_fakeRepository.AsQueryable()).ToList();

      // Assert
      Assert.Single(result);
      Assert.Equal("Bob", result[0].Name);
   }

   [Fact]
   public void AddCompositeMap_CanBeRemovedAndReAdded()
   {
      // Arrange
      var builder = new QueryBuilder<TestClass>()
         .AddCompositeMap("search", x => x.Name, x => x.Tag)
         .AddCondition("search=John");

      // First build should work
      var result1 = builder.Build(_fakeRepository.AsQueryable()).ToList();
      Assert.Single(result1);

      // Now try with a different composite map (override the previous one)
      var builder2 = new QueryBuilder<TestClass>()
         .AddCompositeMap("search", x => x.Name, x => x.Tag)
         .AddCompositeMap("search", x => (object)x.Id) // Override
         .AddCondition("search=1");

      // Act
      var result2 = builder2.Build(_fakeRepository.AsQueryable()).ToList();

      // Assert
      Assert.Single(result2);
      Assert.Equal(1, result2[0].Id);
   }
}
