using System.Linq;
using Gridify;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EntityFrameworkIntegrationTests.cs;

/// <summary>
/// CollectionNegationAsNotAny: a negated condition or an empty value on a nested collection means no element matches.
/// These pin that EF Core translates it like the equivalent hand-written !Any(..), and that the default is unchanged.
/// </summary>
public class CollectionNegationAsNotAnyTests
{
   private readonly MyDbContext _dbContext = new();

   // null checks and the compatibility layer off, so the expected query can be written by hand (see Issue173Tests)
   private static IGridifyMapper<User> Mapper(bool collectionNegationAsNotAny) =>
      new GridifyMapper<User>(q =>
         {
            q.CollectionNegationAsNotAny = collectionNegationAsNotAny;
            q.DisableCollectionNullChecks = true;
            q.EntityFrameworkCompatibilityLayer = false;
         })
         .AddMap("groupName", u => u.Groups.Select(g => g.Name));

   [Fact]
   public void ApplyFiltering_NotEqualOnCollection_WithCollectionNegationAsNotAny_TranslatesToNotExists_SqlServerProvider()
   {
      var expected = _dbContext.Users.Where(u => !u.Groups.Any(g => g.Name == "test")).ToQueryString();
      var actual = _dbContext.Users.ApplyFiltering("groupName!=test", Mapper(true)).ToQueryString();

      Assert.Equal(expected, actual);
   }

   [Fact]
   public void ApplyFiltering_NotContainsOnCollection_WithCollectionNegationAsNotAny_TranslatesToNotExists_SqlServerProvider()
   {
      var expected = _dbContext.Users.Where(u => !u.Groups.Any(g => g.Name.Contains("test"))).ToQueryString();
      var actual = _dbContext.Users.ApplyFiltering("groupName!*test", Mapper(true)).ToQueryString();

      Assert.Equal(expected, actual);
   }

   [Fact]
   public void ApplyFiltering_EmptyValueOnCollection_WithCollectionNegationAsNotAny_TranslatesToNotExists_SqlServerProvider()
   {
      var expected = _dbContext.Users.Where(u => !u.Groups.Any(g => !string.IsNullOrEmpty(g.Name))).ToQueryString();
      var actual = _dbContext.Users.ApplyFiltering("groupName=", Mapper(true)).ToQueryString();

      Assert.Equal(expected, actual);
   }

   [Fact]
   public void ApplyFiltering_NotEqualOnCollection_ByDefault_IsUnchanged_SqlServerProvider()
   {
      var expected = _dbContext.Users.Where(u => u.Groups.Any(g => g.Name != "test")).ToQueryString();
      var actual = _dbContext.Users.ApplyFiltering("groupName!=test", Mapper(false)).ToQueryString();

      Assert.Equal(expected, actual);
   }
}
