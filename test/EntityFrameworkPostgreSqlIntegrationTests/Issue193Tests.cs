using Gridify;
using Xunit;

namespace EntityFrameworkPostgreSqlIntegrationTests;

public class Issue193Tests
{

   [Fact]
   public void ApplyFiltering_ShouldGenerateMappingsForListOfStrings_CaseInsensitive()
   {
      // arrange
      var dataSource = new List<Test>()
      {
         new() {FavouriteColorList = ["Green", "Blue"]},
         new() {FavouriteColorList = ["White", "Yellow"]},
      }.AsQueryable();


      var expected = dataSource.Where(__Test =>
         __Test.FavouriteColorList.Contains("red", StringComparer.InvariantCultureIgnoreCase) |
         __Test.FavouriteColorList.Contains("blue", StringComparer.InvariantCultureIgnoreCase));

      // act
      GridifyGlobalConfiguration.DisableNullChecks = true;
      var actual = dataSource.ApplyFiltering("FavouriteColorList=red/i|FavouriteColorList=blue/i");
      GridifyGlobalConfiguration.DisableNullChecks = false;

      // assert
      Assert.NotEmpty(expected.ToList());
      Assert.Equal(expected.ToString(), actual.ToString());
      var actualList = actual.ToList();
      Assert.Equal(expected.ToList(), actualList);
      Assert.Single(actualList);
   }
   class Test
   {
      public string[] FavouriteColorList { get; set; }
   }
}
