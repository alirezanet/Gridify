using System.Globalization;
using EntityFrameworkIntegrationTests.cs;
using Gridify;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EntityFrameworkPostgreSqlIntegrationTests;

public class Issue284Tests
{
   private readonly MyDbContext _dbContext = new();

   [Fact]
   public void OrderBy_Indexable
      ()
   {
      // arrange
      var gq = new GridifyQuery() { OrderBy = "numericProperties" };
      var mapper = new EntryFilterMapper();
      var expected = _dbContext.Users.OrderBy(x =>
          x.Properties.RootElement.GetProperty("numericProperties").GetDouble()).ToQueryString();

      // act
      var query = _dbContext.Users.ApplyOrdering(gq, mapper).ToQueryString();

      // assert
      Assert.Equal(expected, query);
   }


   private class EntryFilterMapper : GridifyMapper<User>
   {
      public EntryFilterMapper()
      {
         GenerateMappings();
         AddMap("numericProperties", (target, key) => target.Properties
            .RootElement.GetProperty(key).GetDouble(), p => double.Parse(p, CultureInfo.InvariantCulture));
         AddMap("textProperties", (target, key) => target.Properties.RootElement.GetProperty(key).GetString());
      }
   }
}
