using System;
using System.Linq;
using Gridify;
using Xunit;

namespace EntityFrameworkIntegrationTests.cs
{
   public class GridifyEntityFrameworkTests
   {
      private readonly MyDbContext _dbContext;

      public GridifyEntityFrameworkTests()
      {
         _dbContext = new MyDbContext();
         AddTestUsers();
      }

      [Fact]
      public void EntityFrameworkServiceProviderCachingShouldNotThrowException()
      {
         // System.ArgumentException: An item with the same key has already been added. Key: Param_0

         // arrange
         var gq = new GridifyQuery { Filter = "name=n1|name=n2" };

         _dbContext.Users.Gridify(gq);
         _dbContext.Users.Gridify(gq);

         //act
         var exception = Record.Exception(() => _dbContext.Users.GridifyQueryable(gq));

         // assert
         Assert.Null(exception);
      }

      [Fact]
      public void GridifyQueryableDateTimeShouldNotThrowException()
      {
         // arrange
         var gq = new GridifyQuery { OrderBy = "CreateDate" };

         // act
         var exception = Record.Exception(() => _dbContext.Users.GridifyQueryable(gq));

         // assert
         Assert.Null(exception);
      }

      // issue #27 ef core feedback
      // here is working without using the `EnableEntityFrameworkCompatibilityLayer`
      // because EF In-Memory provider can support the StringComparison parameter
      // but it doesn't work in other sql providers
      // https://github.com/alirezanet/Gridify/issues/27#issuecomment-929221457
      [Fact]
      public void ApplyFiltering_GreaterThanBetweenTwoStringsInEF()
      {
         GridifyGlobalConfiguration.EnableEntityFrameworkCompatibilityLayer();

         var actual = _dbContext.Users.ApplyFiltering("name > h").ToList();
         var expected = _dbContext.Users.Where(q => string.Compare(q.Name, "h") > 0).ToList();

         Assert.Equal(expected.Count, actual.Count);
         Assert.Equal(expected, actual);
         Assert.True(actual.Any());
      }
      

      private void AddTestUsers()
      {
         _dbContext.Users.AddRange(
            new User() { Name = "ahmad" },
            new User() { Name = "ali" },
            new User() { Name = "vahid" },
            new User() { Name = "hamid" },
            new User() { Name = "Hamed" },
            new User() { Name = "sara" },
            new User() { Name = "Ali" });

         _dbContext.SaveChanges();
      }
   }
}