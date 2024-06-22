using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Xunit;

namespace Gridify.Tests.IssueTests;

public class Issue168Tests
{
   [Fact]
   [SuppressMessage("ReSharper", "InconsistentNaming")]
   public void IssueStory()
   {
      // arrange
      var dataSource = new List<Artist>()
      {
         new() {FavouriteColors = ["Green", "Blue"]},
         new() {FavouriteColors = ["White", "Yellow"]},
      }.AsQueryable();

      var mapper = new GridifyMapper<Artist>()
         .AddMap("FavouriteColors", w => w.FavouriteColors.Select(Param_0 => Param_0));
      var expected = dataSource.ApplyFiltering("FavouriteColors=Red|FavouriteColors=Blue", mapper);

      // act
      var actual = dataSource.ApplyFiltering("FavouriteColors=Red|FavouriteColors=Blue");

      // assert
      Assert.Equal(expected.ToString(), actual.ToString());
      var actualList = actual.ToList();
      Assert.Equal(expected.ToList(), actualList);
      Assert.Single(actualList);
   }
   private class Artist
   {
      public List<string> FavouriteColors { get; set; } = [];
   }
}
