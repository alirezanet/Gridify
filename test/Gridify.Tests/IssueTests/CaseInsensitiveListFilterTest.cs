using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Gridify.Tests.IssueTests;

public class CaseInsensitiveListFilterTest
{
   [Fact]
   public void ApplyFiltering_WithCaseInsensitiveOperator_OnListOfStrings_ShouldReturnExpectedResult()
   {
      // arrange
      var dataSource = new List<TestClass>()
      {
         new() { FavouriteColorList = ["Green", "Blue"] },
         new() { FavouriteColorList = ["White", "Yellow"] },
         new() { FavouriteColorList = ["Red", "Orange"] },
      }.AsQueryable();

      // act - filtering with /i flag for case-insensitive
      var actual = dataSource.ApplyFiltering("FavouriteColorList=blue/i").ToList();

      // assert
      Assert.Single(actual);
      Assert.Contains("Blue", actual[0].FavouriteColorList);
   }

   [Fact]
   public void ApplyFiltering_WithCaseInsensitiveOperator_OnArrayOfStrings_ShouldReturnExpectedResult()
   {
      // arrange
      var dataSource = new List<TestClass>()
      {
         new() { FavouriteColorsArray = ["Green", "Blue"] },
         new() { FavouriteColorsArray = ["White", "Yellow"] },
         new() { FavouriteColorsArray = ["Red", "Orange"] },
      }.AsQueryable();

      // act - filtering with /i flag for case-insensitive
      var actual = dataSource.ApplyFiltering("FavouriteColorsArray=blue/i").ToList();

      // assert
      Assert.Single(actual);
      Assert.Contains("Blue", actual[0].FavouriteColorsArray);
   }

   [Fact]
   public void ApplyFiltering_WithDefaultCaseInsensitiveFiltering_OnListOfStrings_ShouldReturnExpectedResult()
   {
      // arrange
      var dataSource = new List<TestClass>()
      {
         new() { FavouriteColorList = ["Green", "Blue"] },
         new() { FavouriteColorList = ["White", "Yellow"] },
         new() { FavouriteColorList = ["Red", "Orange"] },
      }.AsQueryable();

      var mapper = new GridifyMapper<TestClass>(q => q.CaseInsensitiveFiltering = true).GenerateMappings();

      // act - filtering without /i flag but with mapper configured for case-insensitive
      var actual = dataSource.ApplyFiltering("FavouriteColorList=blue", mapper).ToList();

      // assert
      Assert.Single(actual);
      Assert.Contains("Blue", actual[0].FavouriteColorList);
   }

   private class TestClass
   {
      public List<string> FavouriteColorList { get; set; } = [];
      public string[] FavouriteColorsArray { get; set; } = [];
   }
}
