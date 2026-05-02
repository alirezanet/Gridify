#nullable enable
using System.Linq;
using Gridify;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EntityFrameworkIntegrationTests.cs;

public class SelectSqlTests
{
   private readonly MyDbContext _dbContext = new();

   [Fact]
   public void ApplySelect_GeneratesColumnPrunedSql()
   {
      var sql = _dbContext.Users.ApplySelect("name").ToQueryString();

      // SELECT must mention the projected column.
      Assert.Contains("[Name]", sql);
      // It must not mention columns the user did not request.
      Assert.DoesNotContain("[CreateDate]", sql);
      Assert.DoesNotContain("[FkGuid]", sql);
   }

   [Fact]
   public void ApplySelect_NullOrWhitespace_GeneratesNormalSelect()
   {
      var sql = _dbContext.Users.ApplySelect((string?)null).ToQueryString();
      Assert.StartsWith("SELECT", sql);
   }

   [Fact]
   public void ApplySelect_TwoFields_BothColumnsAppearOthersDoNot()
   {
      var sql = _dbContext.Users.ApplySelect("name,id").ToQueryString();

      Assert.Contains("[Name]", sql);
      Assert.Contains("[Id]", sql);
      Assert.DoesNotContain("[CreateDate]", sql);
      Assert.DoesNotContain("[FkGuid]", sql);
   }

   [Fact]
   public void ApplyFiltering_ThenApplySelect_FilterAndProjectionBothAppear()
   {
      var gq = new GridifyQuery { Filter = "name=John", Select = "name,id" };
      var sql = _dbContext.Users.ApplyFiltering(gq).ApplySelect((IGridifySelecting)gq).ToQueryString();

      Assert.Contains("WHERE", sql);
      Assert.Contains("[Name]", sql);
      Assert.DoesNotContain("[CreateDate]", sql);
      Assert.DoesNotContain("[FkGuid]", sql);
   }

   [Fact]
   public void ApplySelect_CollectionProjection_IsTranslatedToSql()
   {
      // Project a navigation collection (User.Groups[].Name). This must translate to SQL
      // (a JOIN or correlated subquery). If Enumerable.Select is used in a way EF Core
      // cannot translate, the query falls back to client-side evaluation or throws —
      // ToQueryString() in particular throws InvalidOperationException on untranslatable
      // expressions, which is what this test guards against.
      var sql = _dbContext.Users.ApplySelect("name,groups.name").ToQueryString();

      // The User.Name column must appear; columns not requested must not.
      Assert.Contains("[Name]", sql);
      Assert.DoesNotContain("[CreateDate]", sql);
      Assert.DoesNotContain("[FkGuid]", sql);
   }
}
