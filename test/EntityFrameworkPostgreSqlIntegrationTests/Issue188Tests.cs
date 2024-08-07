using EntityFrameworkIntegrationTests.cs;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Gridify.Tests;

public class Issue188Tests
{

   private readonly MyDbContext _dbContext = new();

   [Fact]
   public void DateTimeFilteringWithUTCKind_UsingGridifyMapper_ShouldNotThrowException()
   {
      var mapper = new GridifyMapper<User>(q => q.DefaultDateTimeKind = DateTimeKind.Utc)
         .GenerateMappings();
      _dbContext.Users.ApplyFiltering("CreateDate>2023-11-14", mapper).ToQueryString();
   }

   [Fact]
   public void DateTimeFilteringWithUTCKind_UsingGlobalConfiguration_ShouldNotThrowException()
   {
      GridifyGlobalConfiguration.DefaultDateTimeKind = DateTimeKind.Utc;
      _dbContext.Users.ApplyFiltering("CreateDate>2023-11-14").ToQueryString();
      GridifyGlobalConfiguration.DefaultDateTimeKind = null;
   }

}


