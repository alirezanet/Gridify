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
      }

      [Fact]
      public void GridifyQueryableDateTimeShouldNotThrowException()
      {
         // arrange
         var gq = new GridifyQuery {IsSortAsc = true, SortBy = "CreateDate"};

         // act
         var exception = Record.Exception(() => _dbContext.Users.GridifyQueryable(gq));

         // assert
         Assert.Null(exception);
      }
   }
}