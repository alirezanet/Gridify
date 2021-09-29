using System;
using System.Linq;
using Gridify;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Xunit;

namespace EntityFrameworkIntegrationTests.cs
{
   public class GridifyEntityFrameworkTests
   {
      private readonly MyDbContext _dbContext;

      public GridifyEntityFrameworkTests()
      {
         _dbContext = new MyDbContext();
         GridifyGlobalConfiguration.EnableEntityFrameworkCompatibilityLayer();
      }

      // issue #25 
      // https://github.com/alirezanet/Gridify/issues/24
      [Fact]
      public void ApplyFiltering_GeneratedSqlShouldMatch_SqlServerProvider()
      {
         var actual = _dbContext.Users.ApplyFiltering("name = vahid").ToQueryString();
         var expected = _dbContext.Users.Where(q => q.Name == "vahid").ToQueryString();

         Assert.Equal(expected , actual);
      }
      

   }
}