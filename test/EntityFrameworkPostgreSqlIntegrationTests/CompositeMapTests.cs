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
      var queryString = _dbContext.Users.ApplyFiltering("search=John", mapper).ToQueryString();

      // Assert
      Assert.NotNull(queryString);
      Assert.Contains("WHERE", queryString, StringComparison.OrdinalIgnoreCase);
      // Verify OR condition between Name and Id
      Assert.Contains("OR", queryString, StringComparison.OrdinalIgnoreCase);
      // Verify both columns are referenced
      Assert.Contains("\"Name\"", queryString);
      Assert.Contains("\"Id\"", queryString);
      // Verify parameter usage
      Assert.Contains("@__p_", queryString);
   }

   [Fact]
   public void CompositeMap_WithTwoStringProperties_ShouldGenerateValidSQL()
   {
      // Arrange
      var mapper = new GridifyMapper<User>()
         .AddCompositeMap("search", x => x.Name);

      // Act
      var queryString = _dbContext.Users.ApplyFiltering("search=TestUser", mapper).ToQueryString();

      // Assert
      Assert.NotNull(queryString);
      Assert.Contains("WHERE", queryString, StringComparison.OrdinalIgnoreCase);
      // Verify Name column is referenced
      Assert.Contains("\"Name\"", queryString);
      // Verify equality operator
      Assert.Contains("=", queryString);
      // Verify parameter usage
      Assert.Contains("@__p_", queryString);
   }

   [Fact]
   public void CompositeMap_StartsWith_ShouldGenerateValidSQL()
   {
      // Arrange
      var mapper = new GridifyMapper<User>(cfg => cfg.CaseInsensitiveFiltering = true)
         .AddCompositeMap("search", x => x.Name);

      // Act
      var queryString = _dbContext.Users.ApplyFiltering("search^John", mapper).ToQueryString();

      // Assert
      Assert.NotNull(queryString);
      Assert.Contains("WHERE", queryString, StringComparison.OrdinalIgnoreCase);
      // Verify Name column is referenced
      Assert.Contains("\"Name\"", queryString);
      // Verify LIKE pattern for StartsWith
      Assert.Contains("LIKE", queryString, StringComparison.OrdinalIgnoreCase);
      // Verify case insensitive function
      Assert.Contains("LOWER", queryString, StringComparison.OrdinalIgnoreCase);
   }

   [Fact]
   public void CompositeMap_EndsWith_ShouldGenerateValidSQL()
   {
      // Arrange
      var mapper = new GridifyMapper<User>(cfg => cfg.CaseInsensitiveFiltering = true)
         .AddCompositeMap("search", x => x.Name);

      // Act
      var queryString = _dbContext.Users.ApplyFiltering("search$son", mapper).ToQueryString();

      // Assert
      Assert.NotNull(queryString);
      Assert.Contains("WHERE", queryString, StringComparison.OrdinalIgnoreCase);
      // Verify Name column is referenced
      Assert.Contains("\"Name\"", queryString);
      // Verify LIKE pattern for EndsWith
      Assert.Contains("LIKE", queryString, StringComparison.OrdinalIgnoreCase);
      // Verify case insensitive function
      Assert.Contains("LOWER", queryString, StringComparison.OrdinalIgnoreCase);
   }

   [Fact]
   public void CompositeMap_Like_Contains_ShouldGenerateValidSQL()
   {
      // Arrange
      var mapper = new GridifyMapper<User>()
         .AddCompositeMap("search", x => x.Name);

      // Act
      var queryString = _dbContext.Users.ApplyFiltering("search=*oh*", mapper).ToQueryString();

      // Assert
      Assert.NotNull(queryString);
      Assert.Contains("WHERE", queryString, StringComparison.OrdinalIgnoreCase);
      // Verify Name column is referenced
      Assert.Contains("\"Name\"", queryString);
      // Verify LIKE operator for contains
      Assert.Contains("LIKE", queryString, StringComparison.OrdinalIgnoreCase);
      // Verify parameter usage
      Assert.Contains("@__p_", queryString);
   }

   [Fact]
   public void CompositeMap_WithNumericProperty_GreaterThan_ShouldGenerateValidSQL()
   {
      // Arrange
      var mapper = new GridifyMapper<User>()
         .AddCompositeMap("search", x => x.Id);

      // Act
      var queryString = _dbContext.Users.ApplyFiltering("search>5", mapper).ToQueryString();

      // Assert
      Assert.NotNull(queryString);
      Assert.Contains("WHERE", queryString, StringComparison.OrdinalIgnoreCase);
      // Verify Id column is referenced
      Assert.Contains("\"Id\"", queryString);
      // Verify greater than operator
      Assert.Contains(">", queryString);
      // Verify parameter usage
      Assert.Contains("@__p_", queryString);
   }

   [Fact]
   public void CompositeMap_WithNumericProperty_LessThan_ShouldGenerateValidSQL()
   {
      // Arrange
      var mapper = new GridifyMapper<User>()
         .AddCompositeMap("search", x => (object)x.Id);

      // Act
      var queryString = _dbContext.Users.ApplyFiltering("search<10", mapper).ToQueryString();

      // Assert
      Assert.NotNull(queryString);
      Assert.Contains("WHERE", queryString, StringComparison.OrdinalIgnoreCase);
      // Verify Id column is referenced
      Assert.Contains("\"Id\"", queryString);
      // Verify less than operator
      Assert.Contains("<", queryString);
      // Verify parameter usage
      Assert.Contains("@__p_", queryString);
   }

   [Fact]
   public void CompositeMap_WithNumericProperty_GreaterOrEqual_ShouldGenerateValidSQL()
   {
      // Arrange
      var mapper = new GridifyMapper<User>()
         .AddCompositeMap("search", x => (object)x.Id);

      // Act
      var queryString = _dbContext.Users.ApplyFiltering("search>=3", mapper).ToQueryString();

      // Assert
      Assert.NotNull(queryString);
      Assert.Contains("WHERE", queryString, StringComparison.OrdinalIgnoreCase);
      // Verify Id column is referenced
      Assert.Contains("\"Id\"", queryString);
      // Verify greater than or equal operator
      Assert.Contains(">=", queryString);
      // Verify parameter usage
      Assert.Contains("@__p_", queryString);
   }

   [Fact]
   public void CompositeMap_WithNumericProperty_LessOrEqual_ShouldGenerateValidSQL()
   {
      // Arrange
      var mapper = new GridifyMapper<User>()
         .AddCompositeMap("search", x => (object)x.Id);

      // Act
      var queryString = _dbContext.Users.ApplyFiltering("search<=7", mapper).ToQueryString();

      // Assert
      Assert.NotNull(queryString);
      Assert.Contains("WHERE", queryString, StringComparison.OrdinalIgnoreCase);
      // Verify Id column is referenced
      Assert.Contains("\"Id\"", queryString);
      // Verify less than or equal operator
      Assert.Contains("<=", queryString);
      // Verify parameter usage
      Assert.Contains("@__p_", queryString);
   }

   [Fact]
   public void CompositeMap_WithGuidProperty_ShouldGenerateValidSQL()
   {
      // Arrange
      var mapper = new GridifyMapper<User>()
         .AddCompositeMap("search", x => x.FkGuid.ToString(), x => x.Name);

      // Act
      var queryString = _dbContext.Users.ApplyFiltering("search=00000000-0000-0000-0000-000000000001", mapper).ToQueryString();

      // Assert
      Assert.NotNull(queryString);
      Assert.Contains("WHERE", queryString, StringComparison.OrdinalIgnoreCase);
      // Verify OR condition between FkGuid and Name
      Assert.Contains("OR", queryString, StringComparison.OrdinalIgnoreCase);
      // Verify both columns are referenced
      Assert.Contains("\"FkGuid\"", queryString);
      Assert.Contains("\"Name\"", queryString);
   }

   [Fact]
   public void CompositeMap_WithThreeProperties_ShouldGenerateValidSQL()
   {
      // Arrange
      var mapper = new GridifyMapper<User>()
         .AddCompositeMap("search", x => x.Name, x => x.Id.ToString());

      // Act
      var queryString = _dbContext.Users.ApplyFiltering("search=John", mapper).ToQueryString();

      // Assert
      Assert.NotNull(queryString);
      Assert.Contains("WHERE", queryString, StringComparison.OrdinalIgnoreCase);
      // Verify OR condition between properties
      Assert.Contains("OR", queryString, StringComparison.OrdinalIgnoreCase);
      // Verify both columns are referenced
      Assert.Contains("\"Name\"", queryString);
      Assert.Contains("\"Id\"", queryString);
      // Verify parameter usage
      Assert.Contains("@__p_", queryString);
   }

   [Fact]
   public void CompositeMap_WithConvertor_ShouldGenerateValidSQL()
   {
      // Arrange
      var mapper = new GridifyMapper<User>()
         .AddCompositeMap("search", val => val.ToUpper(), x => x.Name.ToUpper());

      // Act
      var queryString = _dbContext.Users.ApplyFiltering("search=john", mapper).ToQueryString();

      // Assert
      Assert.NotNull(queryString);
      Assert.Contains("WHERE", queryString, StringComparison.OrdinalIgnoreCase);
      // Verify Name column is referenced
      Assert.Contains("\"Name\"", queryString);
      // Verify UPPER function is applied
      Assert.Contains("UPPER", queryString, StringComparison.OrdinalIgnoreCase);
   }

   [Fact]
   public void CompositeMap_CombinedWithOtherFilters_UsingOr_ShouldGenerateValidSQL()
   {
      // Arrange
      var mapper = new GridifyMapper<User>()
         .AddCompositeMap("search", x => x.Name)
         .AddMap("id", x => x.Id);

      // Act
      var queryString = _dbContext.Users.ApplyFiltering("search=John|id=5", mapper).ToQueryString();

      // Assert
      Assert.NotNull(queryString);
      Assert.Contains("WHERE", queryString, StringComparison.OrdinalIgnoreCase);
      // Verify OR is used to combine filters
      Assert.Contains("OR", queryString, StringComparison.OrdinalIgnoreCase);
      // Verify both columns are referenced
      Assert.Contains("\"Name\"", queryString);
      Assert.Contains("\"Id\"", queryString);
      // Verify parameters are used
      Assert.Matches(@"@__p_\d+", queryString);
   }

   [Fact]
   public void CompositeMap_CombinedWithOtherFilters_UsingAnd_ShouldGenerateValidSQL()
   {
      // Arrange
      var mapper = new GridifyMapper<User>()
         .AddCompositeMap("search", x => x.Name)
         .AddMap("id", x => x.Id);

      // Act
      var queryString = _dbContext.Users.ApplyFiltering("search=John,id=1", mapper).ToQueryString();

      // Assert
      Assert.NotNull(queryString);
      Assert.Contains("WHERE", queryString, StringComparison.OrdinalIgnoreCase);
      // Verify AND is used to combine filters
      Assert.Contains("AND", queryString, StringComparison.OrdinalIgnoreCase);
      // Verify both columns are referenced
      Assert.Contains("\"Name\"", queryString);
      Assert.Contains("\"Id\"", queryString);
      // Verify equality operators
      Assert.Matches(@"=\s*@__p_", queryString);
   }

   [Fact]
   public void CompositeMap_WithParenthesizedExpression_ShouldGenerateValidSQL()
   {
      // Arrange
      var mapper = new GridifyMapper<User>()
         .AddCompositeMap("search", x => x.Name)
         .AddMap("id", x => x.Id);

      // Act
      var queryString = _dbContext.Users.ApplyFiltering("(search=John|id=2),id<10", mapper).ToQueryString();

      // Assert
      Assert.NotNull(queryString);
      Assert.Contains("WHERE", queryString, StringComparison.OrdinalIgnoreCase);
      // Verify all columns are referenced
      Assert.Contains("\"Name\"", queryString);
      Assert.Contains("\"Id\"", queryString);
      // Verify logical operators
      Assert.Contains("OR", queryString, StringComparison.OrdinalIgnoreCase);
      Assert.Contains("AND", queryString, StringComparison.OrdinalIgnoreCase);
      // Verify less than operator
      Assert.Contains("<", queryString);
   }

   [Fact]
   public void CompositeMap_WithDateTimeProperty_ShouldGenerateValidSQL()
   {
      // Arrange
      var mapper = new GridifyMapper<User>(cfg => cfg.DefaultDateTimeKind = DateTimeKind.Utc)
         .AddCompositeMap("search", x => x.Name, x => (object)x.CreateDate);

      // Act
      var queryString = _dbContext.Users.ApplyFiltering("search=2024-01-01", mapper).ToQueryString();

      // Assert
      Assert.NotNull(queryString);
      Assert.Contains("WHERE", queryString, StringComparison.OrdinalIgnoreCase);
      // Verify OR condition between Name and CreateDate
      Assert.Contains("OR", queryString, StringComparison.OrdinalIgnoreCase);
      // Verify both columns are referenced
      Assert.Contains("\"Name\"", queryString);
      Assert.Contains("\"CreateDate\"", queryString);
   }

   [Fact]
   public void CompositeMap_NotEqual_ShouldGenerateValidSQL()
   {
      // Arrange
      var mapper = new GridifyMapper<User>()
         .AddCompositeMap("search", x => x.Name);

      // Act
      var queryString = _dbContext.Users.ApplyFiltering("search!=John", mapper).ToQueryString();

      // Assert
      Assert.NotNull(queryString);
      Assert.Contains("WHERE", queryString, StringComparison.OrdinalIgnoreCase);
      // Verify Name column is referenced
      Assert.Contains("\"Name\"", queryString);
      // Verify not equal operator
      Assert.Contains("<>", queryString);
      // Verify parameter usage
      Assert.Contains("@__p_", queryString);
   }

   [Fact]
   public void CompositeMap_CaseInsensitive_ShouldGenerateValidSQL()
   {
      // Arrange
      var mapper = new GridifyMapper<User>(cfg => cfg.CaseInsensitiveFiltering = true)
         .AddCompositeMap("search", x => x.Name);

      // Act
      var queryString = _dbContext.Users.ApplyFiltering("search=JOHN", mapper).ToQueryString();

      // Assert
      Assert.NotNull(queryString);
      Assert.Contains("WHERE", queryString, StringComparison.OrdinalIgnoreCase);
      // Verify Name column is referenced
      Assert.Contains("\"Name\"", queryString);
      // Verify case insensitive function is applied
      Assert.Contains("LOWER", queryString, StringComparison.OrdinalIgnoreCase);
   }

   [Fact]
   public void CompositeMap_WithOrdering_ShouldGenerateValidSQL()
   {
      // Arrange
      var mapper = new GridifyMapper<User>()
         .AddCompositeMap("search", x => x.Name)
         .AddMap("name", x => x.Name);

      // Act - Apply filtering and ordering
      var query = _dbContext.Users.ApplyFiltering("search=John", mapper);
      var queryString = query.ApplyOrdering("name", mapper).ToQueryString();

      // Assert
      Assert.NotNull(queryString);
      Assert.Contains("WHERE", queryString, StringComparison.OrdinalIgnoreCase);
      Assert.Contains("ORDER BY", queryString, StringComparison.OrdinalIgnoreCase);
      // Verify Name column is referenced in both WHERE and ORDER BY
      Assert.Contains("\"Name\"", queryString);
      // Verify parameter usage
      Assert.Contains("@__p_", queryString);
   }

   [Fact]
   public void CompositeMap_WithPaging_ShouldGenerateValidSQL()
   {
      // Arrange
      var mapper = new GridifyMapper<User>()
         .AddCompositeMap("search", x => x.Name);
      var gridifyQuery = new GridifyQuery { Filter = "search=John", Page = 1, PageSize = 10 };

      // Act
      var queryString = _dbContext.Users.ApplyFiltering(gridifyQuery.Filter, mapper)
         .ApplyPaging(gridifyQuery.Page, gridifyQuery.PageSize)
         .ToQueryString();

      // Assert
      Assert.NotNull(queryString);
      Assert.Contains("WHERE", queryString, StringComparison.OrdinalIgnoreCase);
      Assert.Contains("OFFSET", queryString, StringComparison.OrdinalIgnoreCase);
      Assert.Contains("LIMIT", queryString, StringComparison.OrdinalIgnoreCase);
      // Verify Name column is referenced
      Assert.Contains("\"Name\"", queryString);
      // Verify parameter usage
      Assert.Contains("@__p_", queryString);
   }

   [Fact]
   public void CompositeMap_ComplexExpression_ShouldGenerateValidSQL()
   {
      // Arrange
      var mapper = new GridifyMapper<User>(cfg => cfg.DefaultDateTimeKind = DateTimeKind.Utc)
         .AddCompositeMap("search", x => x.Name, x => x.Id.ToString())
         .AddMap("createDate", x => x.CreateDate);

      // Act
      var queryString = _dbContext.Users.ApplyFiltering("(search=John|search=5),createDate>2020-01-01", mapper).ToQueryString();

      // Assert
      Assert.NotNull(queryString);
      Assert.Contains("WHERE", queryString, StringComparison.OrdinalIgnoreCase);
      // Verify all columns are referenced
      Assert.Contains("\"Name\"", queryString);
      Assert.Contains("\"Id\"", queryString);
      Assert.Contains("\"CreateDate\"", queryString);
      // Verify logical operators
      Assert.Contains("OR", queryString, StringComparison.OrdinalIgnoreCase);
      Assert.Contains("AND", queryString, StringComparison.OrdinalIgnoreCase);
      // Verify greater than operator
      Assert.Contains(">", queryString);
   }
}
