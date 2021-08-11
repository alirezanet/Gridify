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
      public void EntityFrameworkServiceProviderCachingShouldNotThrowException()
      {
         // System.ArgumentException: An item with the same key has already been added. Key: Param_0
        
         // arrange
         var gq = new GridifyQuery { Filter = "name=n1|name=n2"};
         
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
         var gq = new GridifyQuery {OrderBy = "CreateDate"};

         // act
         var exception = Record.Exception(() => _dbContext.Users.GridifyQueryable(gq));

         // assert
         Assert.Null(exception);
      }
   }
}