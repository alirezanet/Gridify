using System.Linq;
using Gridify;
using Microsoft.EntityFrameworkCore;
using xRetry;
using Xunit;

namespace EntityFrameworkIntegrationTests.cs;

public class Issue155Tests
{
   private readonly MyDbContext _dbContext = new();

   // issue #155 - field-to-field comparison should work with Entity Framework
   [RetryFact]
   public void FieldToField_SimpleComparison_ShouldGenerateValidSql()
   {
      // arrange
      GridifyGlobalConfiguration.EnableEntityFrameworkCompatibilityLayer();

      // Map the same field under two different aliases to verify SQL generation
      var mapper = new GridifyMapper<User>(new GridifyMapperConfiguration
      { AllowFieldToFieldComparison = true, EntityFrameworkCompatibilityLayer = true })
         .AddMap("id", u => u.Id)
         .AddMap("sameId", u => u.Id);

      var expected = _dbContext.Users.Where(u => u.Id == u.Id).ToQueryString();

      // act
      var actual = _dbContext.Users
         .ApplyFiltering("id=(sameId)", mapper)
         .ToQueryString();

      // assert - verify the generated SQL is the same as the LINQ query
      Assert.Equal(expected, actual);
   }

   // issue #155 - field-to-field comparison with nested collection should work with EF
   [RetryFact]
   public void FieldToField_NestedCollectionComparison_ShouldGenerateValidSql()
   {
      // arrange
      GridifyGlobalConfiguration.EnableEntityFrameworkCompatibilityLayer();

      var mapper = new GridifyMapper<User>(new GridifyMapperConfiguration
      { AllowFieldToFieldComparison = true, EntityFrameworkCompatibilityLayer = true })
         .AddMap("groupId", u => u.Groups.Select(g => g.Id))
         .AddMap("groupSameId", u => u.Groups.Select(g => g.Id));

      var expected = _dbContext.Users
         .Where(u => u.Groups.Any(g => g.Id == g.Id))
         .ToQueryString();

      // act
      var actual = _dbContext.Users
         .ApplyFiltering("groupId=(groupSameId)", mapper)
         .ToQueryString();

      // assert
      Assert.Equal(expected, actual);
   }
}
