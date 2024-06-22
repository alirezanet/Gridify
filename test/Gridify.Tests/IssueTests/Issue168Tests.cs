using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Xunit;

namespace Gridify.Tests.IssueTests;

[SuppressMessage("ReSharper", "InconsistentNaming")]
[SuppressMessage("ReSharper", "ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract")]
public class Issue168Tests
{
   [Fact]
   public void ApplyFiltering_WithoutMapper_ShouldGenerateMappingsForListOfStrings()
   {
      // arrange
      var dataSource = new List<Test>()
      {
         new() {FavouriteColorList = ["Green", "Blue"]},
         new() {FavouriteColorList = ["White", "Yellow"]},
      }.AsQueryable();

      var mapper = new GridifyMapper<Test>()
         .AddMap("FavouriteColorList", w => w.FavouriteColorList.Select(Param_0 => Param_0));
      var expected = dataSource.ApplyFiltering("FavouriteColorList=Red|FavouriteColorList=Blue", mapper);

      // act
      var actual = dataSource.ApplyFiltering("FavouriteColorList=Red|FavouriteColorList=Blue");

      // assert
      Assert.Equal(expected.ToString(), actual.ToString());
      var actualList = actual.ToList();
      Assert.Equal(expected.ToList(), actualList);
      Assert.Single(actualList);
   }

   [Fact]
   public void GenerateMappings_ShouldGenerateMappingsForSimpleTypes()
   {
      var gm = new GridifyMapper<Test>()
         .GenerateMappings();

      Assert.Equal(typeof(Test).GetProperties().Length, gm.GetCurrentMaps().Count());
   }

   [Fact]
   public void ApplyFiltering_WithoutMapper_ShouldGenerateMappingsForArrayOfStrings()
   {
      // arrange
      var dataSource = new List<Test>()
      {
         new() {FavouriteColorsArray = ["Green", "Blue"]},
         new() {FavouriteColorsArray = ["White", "Yellow"]},
      }.AsQueryable();

      var mapper = new GridifyMapper<Test>()
         .AddMap("FavouriteColorsArray", w => w.FavouriteColorsArray.Select(Param_0 => Param_0));
      var expected = dataSource.ApplyFiltering("FavouriteColorsArray=Red|FavouriteColorsArray=Blue", mapper);

      // act
      var actual = dataSource.ApplyFiltering("FavouriteColorsArray=Red|FavouriteColorsArray=Blue");

      // assert
      Assert.Equal(expected.ToString(), actual.ToString());
      var actualList = actual.ToList();
      Assert.Equal(expected.ToList(), actualList);
      Assert.Single(actualList);
   }

   [Fact]
   public void ApplyFiltering_WithoutMapper_ShouldGenerateTheCorrectQuery()
   {
      // arrange
      var dataSource = new List<Test>()
      {
         new() {NumbersArray = [1, 2]},
         new() {NumbersArray = [3, 4]},
      }.AsQueryable();

      var expected = dataSource.Where(__Test => __Test.NumbersArray != null && __Test.NumbersArray.Contains(2));

      // act
      var actual = dataSource.ApplyFiltering("NumbersArray=2");

      // assert
      Assert.Equal(expected.ToString(), actual.ToString());
      var actualList = actual.ToList();
      Assert.Equal(expected.ToList(), actualList);
      Assert.Single(actualList);
      GridifyGlobalConfiguration.DisableNullChecks = false;
   }

   [Fact]
   public void ApplyFiltering_WithoutMapperWithMultipleConditions_ShouldGenerateTheCorrectQuery()
   {
      // arrange
      var dataSource = new List<Test>()
      {
         new() {NumbersArray = [1, 2]},
         new() {NumbersArray = [3, 4]},
      }.AsQueryable();

      var expected = dataSource.Where(__Test =>
         __Test.NumbersArray != null && __Test.NumbersArray.Contains(2) || __Test.NumbersArray != null && __Test.NumbersArray.Contains(5));

      // act
      var actual = dataSource.ApplyFiltering("NumbersArray=2|NumbersArray=5");

      // assert
      Assert.Equal(expected.ToString(), actual.ToString());
      var actualList = actual.ToList();
      Assert.Equal(expected.ToList(), actualList);
      Assert.Single(actualList);
      GridifyGlobalConfiguration.DisableNullChecks = false;
   }

   [Fact]
   public void ApplyFiltering_WithoutMapper_ShouldGenerateMappingsForEnumArray()
   {
      // arrange
      var dataSource = new List<Test>()
      {
         new() {EnumsArray = [Test.MyEnum.Item1, Test.MyEnum.Item2]},
         new() {EnumsArray = [Test.MyEnum.Item1]},
      }.AsQueryable();

      var expected = dataSource.Where(__Test =>
         __Test.EnumsArray != null && __Test.EnumsArray.Contains(Test.MyEnum.Item1) ||
         __Test.EnumsArray != null && __Test.EnumsArray.Contains(Test.MyEnum.Item2));

      // act
      var actual = dataSource.ApplyFiltering("EnumsArray=Item1|EnumsArray=Item2");

      // assert
      Assert.Equal(expected.ToString(), actual.ToString());
      var actualList = actual.ToList();
      Assert.Equal(expected.ToList(), actualList);
      Assert.Equal(2, actualList.Count);
   }


   private class Test
   {
      public List<string> FavouriteColorList { get; set; } = [];
      public string[] FavouriteColorsArray { get; set; } = [];

      public int[] NumbersArray { get; set; } = [];
      public IEnumerable<long> NumbersEnumerable { get; set; } = [];
      public MyEnum[] EnumsArray { get; set; } = [];

      public enum MyEnum
      {
         Item1,
         Item2
      }

   }
}
