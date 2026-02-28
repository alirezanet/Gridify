using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Gridify.Tests.IssueTests;

public class Issue204Tests
{
   [Fact]
   public void ApplyFiltering_NotEquals_ShouldMatch_NullItems()
   {
      // arrange
      var dataSource = new List<Test>()
        {
            new() {FavouriteColorList = ["Green", "Blue"]},
            new() {FavouriteColorList = ["White", "Yellow"]},
            new() { FavouriteColorList = null },
        }.AsQueryable();

      var expected = dataSource.Where(q => q.FavouriteColorList == null || !q.FavouriteColorList.Contains("Green")).ToList();
      var actual = dataSource.ApplyFiltering("FavouriteColorList!=Green").ToList();

      // assert
      Assert.Equal(expected.Count, actual.Count);
      Assert.Equal(expected, actual);
      Assert.True(actual.Any());
   }

   private class Test
   {
      public List<string>? FavouriteColorList { get; set; }
   }
}
