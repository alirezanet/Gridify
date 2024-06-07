using System.Linq;
using Gridify;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EntityFrameworkIntegrationTests.cs;

public class Issue173Tests
{

   private readonly MyDbContext _dbContext = new();

   [Fact]
   public void EF_ManyToManyQuery_ShouldNotThrow()
   {
      // arrange
      GridifyGlobalConfiguration.EnableEntityFrameworkCompatibilityLayer();

      var group = new { Name =  "test group" };
      var expected = _dbContext.Users.Where(u => u.Groups.Any(g => g.Name == group.Name)).ToQueryString();

      // act
      var actual = new QueryBuilder<User>()
         .AddMap("groupName", u => u.Groups.Select(q => q.Name))
         .AddCondition("groupName=test group")
         .Build(_dbContext.Users)
         .ToQueryString();

      // assert
      Assert.Equal(expected, actual.Replace(" @__Value_0", " @__group_Name_0"));
   }
}
