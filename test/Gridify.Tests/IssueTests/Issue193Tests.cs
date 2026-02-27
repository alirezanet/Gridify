using System;
using System.Collections.Generic;
using System.Linq;
using Gridify;
using Xunit;

namespace Gridify.Tests.IssueTests;

public class Issue193Tests
{

   [Fact]
   public void ApplyFiltering_WithCaseInsensitiveOperator_ShouldReturnExpectedResult()
   {
      // arrange
      var dataSource = Test.GetTestDataSource();

      var expected = dataSource.Where(q => q.FavouriteColorList.Select(c => c.ToLower()).Contains("red") |
                                           q.FavouriteColorList.Select(c => c.ToLower()).Contains("blue"))
         .ToList();

      // act
      var actual = dataSource.ApplyFiltering("FavouriteColorList=red/i|FavouriteColorList=blue/i").ToList();

      // assert
      Assert.NotEmpty(expected);
      Assert.NotEmpty(actual);
      Assert.Equal(expected.Count, actual.Count);
   }

   [Fact]
   public void ApplyFiltering_WithDefaultCaseInsensitiveFiltering_ShouldReturnExpectedResult()
   {
      // arrange
      var dataSource = Test.GetTestDataSource();

      var expected = dataSource.Where(q => q.FavouriteColorList.Select(c => c.ToLower()).Contains("red") |
                                           q.FavouriteColorList.Select(c => c.ToLower()).Contains("blue"))
         .ToList();

      var mapper = new GridifyMapper<Test>(q => q.CaseInsensitiveFiltering = true).GenerateMappings();

      // act
      var actual = dataSource.ApplyFiltering("FavouriteColorList=red|FavouriteColorList=blue", mapper).ToList();

      // assert
      Assert.NotEmpty(expected);
      Assert.NotEmpty(actual);
      Assert.Equal(expected.Count, actual.Count);
   }

   class Test
   {
      public string[] FavouriteColorList { get; set; } = [];

      public static IQueryable<Test> GetTestDataSource()
      {
         return new List<Test>()
         {
            new() { FavouriteColorList = ["Green", "Blue"] },
            new() { FavouriteColorList = ["White", "Yellow"] },
            new() { FavouriteColorList = ["Red", "Orange"] },
            new() { FavouriteColorList = ["Purple", "Pink"] },
            new() { FavouriteColorList = ["Black", "Gray"] }
         }.AsQueryable();
      }
   }
}
