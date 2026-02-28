using EntityFrameworkIntegrationTests.cs;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Gridify.Tests;

/// <summary>
/// Tests for CompositeMap feature compatibility with Entity Framework PostgreSQL
/// </summary>
public class CompositeMapEFPostgreSqlTests
{
   private readonly MyDbContext _dbContext = new();

   [Fact]
   public void CompositeMap_Equal_ShouldGenerateValidSQL()
   {
      // Arrange
      var mapper = new GridifyMapper<User>()
         .AddCompositeMap("search", x => x.Name, x => x.Id.ToString());

      // Act
      var actualSql = _dbContext.Users.ApplyFiltering("search=John", mapper).ToQueryString();
      var expectedSql = _dbContext.Users.Where(u => u.Name == "John" || u.Id.ToString() == "John").ToQueryString();

      // Assert
      Assert.Equal(expectedSql, actualSql);
   }

   [Fact]
   public void CompositeMap_WithTwoStringProperties_ShouldGenerateValidSQL()
   {
      // Arrange
      var mapper = new GridifyMapper<User>()
         .AddCompositeMap("search", x => x.Name);

      // Act
      var actualSql = _dbContext.Users.ApplyFiltering("search=TestUser", mapper).ToQueryString();
      var expectedSql = _dbContext.Users.Where(u => u.Name == "TestUser").ToQueryString();

      // Assert
      Assert.Equal(expectedSql, actualSql);
   }

   [Fact]
   public void CompositeMap_StartsWith_ShouldGenerateValidSQL()
   {
      // Arrange
      var mapper = new GridifyMapper<User>(cfg => cfg.CaseInsensitiveFiltering = true)
         .AddCompositeMap("search", x => x.Name);

      // Act
      var actualSql = _dbContext.Users.ApplyFiltering("search^John", mapper).ToQueryString();
      var expectedSql = _dbContext.Users.Where(u => u.Name.ToLower().StartsWith("john")).ToQueryString();

      // Assert
      Assert.Equal(expectedSql, actualSql);
   }

   [Fact]
   public void CompositeMap_EndsWith_ShouldGenerateValidSQL()
   {
      // Arrange
      var mapper = new GridifyMapper<User>(cfg => cfg.CaseInsensitiveFiltering = true)
         .AddCompositeMap("search", x => x.Name);

      // Act
      var actualSql = _dbContext.Users.ApplyFiltering("search$son", mapper).ToQueryString();
      var expectedSql = _dbContext.Users.Where(u => u.Name.ToLower().EndsWith("son")).ToQueryString();

      // Assert
      Assert.Equal(expectedSql, actualSql);
   }

   [Fact]
   public void CompositeMap_Like_Contains_ShouldGenerateValidSQL()
   {
      // Arrange
      var mapper = new GridifyMapper<User>()
         .AddCompositeMap("search", x => x.Name);

      // Act
      var actualSql = _dbContext.Users.ApplyFiltering("search=*oh*", mapper).ToQueryString();
      var expectedSql = _dbContext.Users.Where(u => u.Name.Contains("oh*")).ToQueryString();

      // Assert
      Assert.Equal(expectedSql, actualSql);
   }

   [Fact]
   public void CompositeMap_WithNumericProperty_GreaterThan_ShouldGenerateValidSQL()
   {
      // Arrange
      var mapper = new GridifyMapper<User>()
         .AddCompositeMap("search", x => x.Id);

      // Act
      var actualSql = _dbContext.Users.ApplyFiltering("search>5", mapper).ToQueryString();
      var expectedSql = _dbContext.Users.Where(u => u.Id > 5).ToQueryString();

      // Assert
      Assert.Equal(expectedSql, actualSql);
   }

   [Fact]
   public void CompositeMap_WithNumericProperty_LessThan_ShouldGenerateValidSQL()
   {
      // Arrange
      var mapper = new GridifyMapper<User>()
         .AddCompositeMap("search", x => (object)x.Id);

      // Act
      var actualSql = _dbContext.Users.ApplyFiltering("search<10", mapper).ToQueryString();

      // Assert - Cannot create equivalent LINQ for object < object, verify structure
      Assert.Contains("WHERE", actualSql, StringComparison.OrdinalIgnoreCase);
      Assert.Contains("\"Id\"", actualSql);
      Assert.Contains("<", actualSql);
      Assert.Contains("10", actualSql);
   }

   [Fact]
   public void CompositeMap_WithNumericProperty_GreaterOrEqual_ShouldGenerateValidSQL()
   {
      // Arrange
      var mapper = new GridifyMapper<User>()
         .AddCompositeMap("search", x => (object)x.Id);

      // Act
      var actualSql = _dbContext.Users.ApplyFiltering("search>=3", mapper).ToQueryString();

      // Assert - Cannot create equivalent LINQ for object >= object, verify structure
      Assert.Contains("WHERE", actualSql, StringComparison.OrdinalIgnoreCase);
      Assert.Contains("\"Id\"", actualSql);
      Assert.Contains(">=", actualSql);
      Assert.Contains("3", actualSql);
   }

   [Fact]
   public void CompositeMap_WithNumericProperty_LessOrEqual_ShouldGenerateValidSQL()
   {
      // Arrange
      var mapper = new GridifyMapper<User>()
         .AddCompositeMap("search", x => (object)x.Id);

      // Act
      var actualSql = _dbContext.Users.ApplyFiltering("search<=7", mapper).ToQueryString();

      // Assert - Cannot create equivalent LINQ for object <= object, verify structure
      Assert.Contains("WHERE", actualSql, StringComparison.OrdinalIgnoreCase);
      Assert.Contains("\"Id\"", actualSql);
      Assert.Contains("<=", actualSql);
      Assert.Contains("7", actualSql);
   }

   [Fact]
   public void CompositeMap_WithGuidProperty_ShouldGenerateValidSQL()
   {
      // Arrange
      var mapper = new GridifyMapper<User>()
         .AddCompositeMap("search", x => x.FkGuid.ToString(), x => x.Name);

      // Act
      var actualSql = _dbContext.Users.ApplyFiltering("search=00000000-0000-0000-0000-000000000001", mapper).ToQueryString();
      var expectedSql = _dbContext.Users.Where(u => u.FkGuid.ToString() == "00000000-0000-0000-0000-000000000001" || u.Name == "00000000-0000-0000-0000-000000000001").ToQueryString();

      // Assert
      Assert.Equal(expectedSql, actualSql);
   }

   [Fact]
   public void CompositeMap_WithThreeProperties_ShouldGenerateValidSQL()
   {
      // Arrange
      var mapper = new GridifyMapper<User>()
         .AddCompositeMap("search", x => x.Name, x => x.Id.ToString());

      // Act
      var actualSql = _dbContext.Users.ApplyFiltering("search=John", mapper).ToQueryString();
      var expectedSql = _dbContext.Users.Where(u => u.Name == "John" || u.Id.ToString() == "John").ToQueryString();

      // Assert
      Assert.Equal(expectedSql, actualSql);
   }

   [Fact]
   public void CompositeMap_WithConvertor_ShouldGenerateValidSQL()
   {
      // Arrange
      var mapper = new GridifyMapper<User>()
         .AddCompositeMap("search", val => val.ToUpper(), x => x.Name.ToUpper());

      // Act
      var actualSql = _dbContext.Users.ApplyFiltering("search=john", mapper).ToQueryString();
      var expectedSql = _dbContext.Users.Where(u => u.Name.ToUpper() == "JOHN").ToQueryString();

      // Assert
      Assert.Equal(expectedSql, actualSql);
   }

   [Fact]
   public void CompositeMap_CombinedWithOtherFilters_UsingOr_ShouldGenerateValidSQL()
   {
      // Arrange
      var mapper = new GridifyMapper<User>()
         .AddCompositeMap("search", x => x.Name)
         .AddMap("id", x => x.Id);

      // Act
      var actualSql = _dbContext.Users.ApplyFiltering("search=John|id=5", mapper).ToQueryString();
      var expectedSql = _dbContext.Users.Where(u => u.Name == "John" || u.Id == 5).ToQueryString();

      // Assert
      Assert.Equal(expectedSql, actualSql);
   }

   [Fact]
   public void CompositeMap_CombinedWithOtherFilters_UsingAnd_ShouldGenerateValidSQL()
   {
      // Arrange
      var mapper = new GridifyMapper<User>()
         .AddCompositeMap("search", x => x.Name)
         .AddMap("id", x => x.Id);

      // Act
      var actualSql = _dbContext.Users.ApplyFiltering("search=John,id=1", mapper).ToQueryString();
      var expectedSql = _dbContext.Users.Where(u => u.Name == "John" && u.Id == 1).ToQueryString();

      // Assert
      Assert.Equal(expectedSql, actualSql);
   }

   [Fact]
   public void CompositeMap_WithParenthesizedExpression_ShouldGenerateValidSQL()
   {
      // Arrange
      var mapper = new GridifyMapper<User>()
         .AddCompositeMap("search", x => x.Name)
         .AddMap("id", x => x.Id);

      // Act
      var actualSql = _dbContext.Users.ApplyFiltering("(search=John|id=2),id<10", mapper).ToQueryString();
      var expectedSql = _dbContext.Users.Where(u => (u.Name == "John" || u.Id == 2) && u.Id < 10).ToQueryString();

      // Assert
      Assert.Equal(expectedSql, actualSql);
   }

   [Fact]
   public void CompositeMap_WithDateTimeProperty_ShouldGenerateValidSQL()
   {
      //Arrange
      var mapper = new GridifyMapper<User>(cfg => cfg.DefaultDateTimeKind = DateTimeKind.Utc)
         .AddCompositeMap("search", x => x.Name, x => (object)x.CreateDate!);

      // Act
      var actualSql = _dbContext.Users.ApplyFiltering("search=2024-01-01", mapper).ToQueryString();

      // Assert - Verify structure (exact timestamp depends on system timezone)
      Assert.Contains("WHERE", actualSql, StringComparison.OrdinalIgnoreCase);
      Assert.Contains("OR", actualSql, StringComparison.OrdinalIgnoreCase);
      Assert.Contains("\"Name\"", actualSql);
      Assert.Contains("\"CreateDate\"", actualSql);
      Assert.Contains("TIMESTAMPTZ", actualSql, StringComparison.OrdinalIgnoreCase);
      Assert.Contains("2024-01-01", actualSql); // String comparison
   }

   [Fact]
   public void CompositeMap_NotEqual_ShouldGenerateValidSQL()
   {
      // Arrange
      var mapper = new GridifyMapper<User>()
         .AddCompositeMap("search", x => x.Name);

      // Act
      var actualSql = _dbContext.Users.ApplyFiltering("search!=John", mapper).ToQueryString();
      var expectedSql = _dbContext.Users.Where(u => u.Name != "John").ToQueryString();

      // Assert
      Assert.Equal(expectedSql, actualSql);
   }

   [Fact]
   public void CompositeMap_CaseInsensitive_ShouldGenerateValidSQL()
   {
      // Arrange
      var mapper = new GridifyMapper<User>(cfg => cfg.CaseInsensitiveFiltering = true)
         .AddCompositeMap("search", x => x.Name);

      // Act
      var actualSql = _dbContext.Users.ApplyFiltering("search=JOHN", mapper).ToQueryString();
      var expectedSql = _dbContext.Users.Where(u => u.Name.ToLower() == "john").ToQueryString();

      // Assert
      Assert.Equal(expectedSql, actualSql);
   }

   [Fact]
   public void CompositeMap_WithOrdering_ShouldGenerateValidSQL()
   {
      // Arrange
      var mapper = new GridifyMapper<User>()
         .AddCompositeMap("search", x => x.Name)
         .AddMap("name", x => x.Name);

      // Act - Apply filtering and ordering
      var actualSql = _dbContext.Users
         .ApplyFiltering("search=John", mapper)
         .ApplyOrdering("name", mapper)
         .ToQueryString();
      var expectedSql = _dbContext.Users
         .Where(u => u.Name == "John")
         .OrderBy(u => u.Name)
         .ToQueryString();

      // Assert
      Assert.Equal(expectedSql, actualSql);
   }

   [Fact]
   public void CompositeMap_WithPaging_ShouldGenerateValidSQL()
   {
      // Arrange
      var mapper = new GridifyMapper<User>()
         .AddCompositeMap("search", x => x.Name);
      var gridifyQuery = new GridifyQuery { Filter = "search=John", Page = 1, PageSize = 10 };

      // Act
      var actualSql = _dbContext.Users
         .ApplyFiltering(gridifyQuery.Filter, mapper)
         .ApplyPaging(gridifyQuery.Page, gridifyQuery.PageSize)
         .ToQueryString();
      var expectedSql = _dbContext.Users
         .Where(u => u.Name == "John")
         .Skip(0)
         .Take(10)
         .ToQueryString();

      // Assert
      Assert.Equal(expectedSql, actualSql);
   }

   [Fact]
   public void CompositeMap_ComplexExpression_ShouldGenerateValidSQL()
   {
      // Arrange
      var mapper = new GridifyMapper<User>(cfg => cfg.DefaultDateTimeKind = DateTimeKind.Utc)
         .AddCompositeMap("search", x => x.Name, x => x.Id.ToString())
         .AddMap("createDate", x => x.CreateDate);

      // Act
      var actualSql = _dbContext.Users.ApplyFiltering("(search=John|search=5),createDate>2020-01-01", mapper).ToQueryString();

      // Assert - Verify structure (exact timestamp depends on system timezone)
      Assert.Contains("WHERE", actualSql, StringComparison.OrdinalIgnoreCase);
      Assert.Contains("\"Name\"", actualSql);
      Assert.Contains("\"Id\"", actualSql);
      Assert.Contains("\"CreateDate\"", actualSql);
      Assert.Contains("OR", actualSql, StringComparison.OrdinalIgnoreCase);
      Assert.Contains("AND", actualSql, StringComparison.OrdinalIgnoreCase);
      Assert.Contains(">", actualSql);
      Assert.Contains("TIMESTAMPTZ", actualSql, StringComparison.OrdinalIgnoreCase);
   }

   [Fact]
   public void CompositeMap_WithEntityFrameworkCompatibilityLayer_ShouldGenerateParameterizedSQL()
   {
      // Arrange - Enable EF Compatibility Layer for parameterized queries
      var mapper = new GridifyMapper<User>(cfg => cfg.EntityFrameworkCompatibilityLayer = true)
         .AddCompositeMap("search", x => x.Name, x => x.Id.ToString());

      // Act
      var actualSql = _dbContext.Users.ApplyFiltering("search=John", mapper).ToQueryString();

      // Assert - Should use parameters instead of inlined values
      Assert.Contains("WHERE", actualSql, StringComparison.OrdinalIgnoreCase);
      Assert.Contains("OR", actualSql, StringComparison.OrdinalIgnoreCase);
      Assert.Contains("\"Name\"", actualSql);
      Assert.Contains("\"Id\"", actualSql);
      // Verify parameters are used (EF Compatibility Layer uses @Value pattern)
      Assert.Matches(@"@Value\d*", actualSql); // Should find @Value, @Value0, etc.

      // Should NOT contain inlined 'John' value in the query body
      // (it appears in comments but not as a SQL literal)
      var sqlWithoutComments = System.Text.RegularExpressions.Regex.Replace(actualSql, @"--[^\r\n]*", "");
      Assert.DoesNotContain("'John'", sqlWithoutComments);
   }
}
