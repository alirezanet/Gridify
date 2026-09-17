using Gridify;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EntityFrameworkIntegrationTests.cs;

/// <summary>
/// issue #332, https://github.com/alirezanet/Gridify/issues/332
/// The contains operator falls back to the member's text for a non-string member, the same way
/// starts with and ends with already did. These pin that EF Core can translate that to SQL,
/// because a fallback the provider cannot translate would be worse than the exception it replaced.
/// </summary>
public class Issue332ContainsOnNonStringTests
{
   private readonly MyDbContext _dbContext = new();

   [Theory]
   [InlineData("Id=*5", "CONVERT(varchar(11), [u].[Id]) LIKE")]
   [InlineData("Id!*5", "CONVERT(varchar(11), [u].[Id]) NOT LIKE")]
   // the two operators that already had the fallback, kept here so the pair cannot drift apart
   [InlineData("Id^3", "CONVERT(varchar(11), [u].[Id]) LIKE")]
   [InlineData("Id$5", "CONVERT(varchar(11), [u].[Id]) LIKE")]
   public void ApplyFiltering_TextOperatorOnANonStringColumn_TranslatesToSql_SqlServerProvider(
      string filter, string expectedSqlFragment)
   {
      var sql = _dbContext.Users.ApplyFiltering(filter).ToQueryString();

      Assert.Contains(expectedSqlFragment, sql);
   }

   [Fact]
   public void ApplyFiltering_ContainsOnAStringColumn_IsUnaffected_SqlServerProvider()
   {
      var sql = _dbContext.Users.ApplyFiltering("Name=*va").ToQueryString();

      Assert.Contains("[u].[Name] LIKE", sql);
      Assert.DoesNotContain("CONVERT", sql);
   }
}
