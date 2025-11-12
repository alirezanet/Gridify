using System.Linq;
using Gridify;
using Microsoft.EntityFrameworkCore;
using xRetry;
using Xunit;

namespace EntityFrameworkIntegrationTests.cs;

public class Issue173Tests
{

   private readonly MyDbContext _dbContext = new();

   [RetryFact]
   public void EF_ManyToManyQuery_ShouldNotThrow()
   {
      // arrange
      GridifyGlobalConfiguration.EnableEntityFrameworkCompatibilityLayer();

      var group = new { Name = "test group" };
      var expected = _dbContext.Users.Where(u => u.Groups.Any(g => g.Name == group.Name)).ToQueryString();

      // act
      var actual = new QueryBuilder<User>()
         .AddMap("groupName", u => u.Groups.Select(q => q.Name))
         .AddCondition("groupName=test group")
         .Build(_dbContext.Users)
         .ToQueryString();

      // assert
      Assert.Equal(expected, actual.Replace(" @Value", " @group_Name"));
   }

   [RetryFact]
   public void EF_ManyToManyQuery_WhenNullCheckAndCompatibilityLayersIsDisabled_ShouldNotThrow()
   {
      // arrange
      GridifyGlobalConfiguration.DisableEntityFrameworkCompatibilityLayer();
      GridifyGlobalConfiguration.DisableNullChecks = true;

      var expected = _dbContext.Users.Where(u => u.Groups.Any(g => g.Name == "test")).ToQueryString();

      // act
      var actual = new QueryBuilder<User>()
         .AddMap("groupName", u => u.Groups.Select(q => q.Name))
         .AddCondition("groupName=test")
         .Build(_dbContext.Users)
         .ToQueryString();

      // reset global configurations
      GridifyGlobalConfiguration.EnableEntityFrameworkCompatibilityLayer();
      GridifyGlobalConfiguration.DisableNullChecks = false;

      // assert
      Assert.Equal(expected, actual);
   }
}
