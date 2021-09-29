using System;
using System.Linq;
using Gridify;
using Microsoft.EntityFrameworkCore;
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

      // issue #24,  
      // https://github.com/alirezanet/Gridify/issues/24
      [Fact]
      public void ApplyFiltering_GeneratedSqlShouldMatch_SqlServerProvider()
      {
         var actual = _dbContext.Users.ApplyFiltering("name = vahid").ToQueryString();
         var expected = _dbContext.Users.Where(q => q.Name == "vahid").ToQueryString();

         Assert.Equal(expected , actual);
      }
      
      // issue #24,  
      // https://github.com/alirezanet/Gridify/issues/24
      [Fact]
      public void ApplyFiltering_GeneratedSqlShouldMatch_UsingVariable_SqlServerProvider()
      {
         var name = "vahid";
         var actual = _dbContext.Users.ApplyFiltering("name = vahid").ToQueryString();
         var expected = _dbContext.Users.Where(q => q.Name == name).ToQueryString();

         Assert.Equal(expected , actual);
      }
      
      // issue #27 ef core sqlServer feedback
      // https://github.com/alirezanet/Gridify/issues/27#issuecomment-929221457
      [Fact]
      public void ApplyFiltering_GreaterThanBetweenTwoStringsInEF_SqlServerProvider()
      {
         // The EntityFrameworkCompatibilityLayer has enabled in the constructor
         var actual = _dbContext.Users.ApplyFiltering("name > h").ToQueryString();
         const string expected = @"SELECT [u].[Id], [u].[CreateDate], [u].[FkGuid], [u].[Name]
FROM [Users] AS [u]
WHERE [u].[Name] > N'h'";
         
         Assert.Equal(expected,actual);
      }
      
      // issue #27 ef core sqlServer feedback
      // https://github.com/alirezanet/Gridify/issues/27#issuecomment-929221457dd
      [Fact]
      public void ApplyFiltering_GreaterThanBetweenTwoStringsInEF_SqlServerProvider_ShouldThrowErrorWhenCompatibilityLayerIsDisabled()
      {
         GridifyGlobalConfiguration.DisableEntityFrameworkCompatibilityLayer();
         Assert.Throws<InvalidOperationException>(() => _dbContext.Users.ApplyFiltering("name > h").ToQueryString());
      }

   }
}