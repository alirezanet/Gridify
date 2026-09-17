using EntityFrameworkIntegrationTests.cs;
using Gridify;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Gridify.Tests;

/// <summary>
/// issue #332, https://github.com/alirezanet/Gridify/issues/332
/// The PostgreSQL counterpart of the SQL Server translation tests: the contains operator on a
/// non-string member has to reach the provider as a cast, not fail to translate.
/// </summary>
public class Issue332ContainsOnNonStringPostgreSqlTests
{
   private readonly MyDbContext _dbContext = new();

   [Theory]
   [InlineData("Id=*5", """u."Id"::text LIKE""")]
   [InlineData("Id!*5", """u."Id"::text NOT LIKE""")]
   // the two operators that already had the fallback, kept here so the pair cannot drift apart
   [InlineData("Id^3", """u."Id"::text LIKE""")]
   [InlineData("Id$5", """u."Id"::text LIKE""")]
   public void ApplyFiltering_TextOperatorOnANonStringColumn_TranslatesToSql_PostgreSqlProvider(
      string filter, string expectedSqlFragment)
   {
      var sql = _dbContext.Users.ApplyFiltering(filter).ToQueryString();

      Assert.Contains(expectedSqlFragment, sql);
   }
}
