using System.Collections.Generic;
using System.Linq;
using Xunit;
using Xunit.Abstractions;

namespace Gridify.Tests;

public class ErrorMessageTests
{
   private readonly ITestOutputHelper _testOutputHelper;

   public ErrorMessageTests(ITestOutputHelper testOutputHelper)
   {
      _testOutputHelper = testOutputHelper;
   }

   [Theory]
   [InlineData("#test=x", 0)]
   [InlineData("t#est=x", 1)]
   [InlineData("te#st=x", 2)]
   [InlineData("tes#t=x", 3)]
   [InlineData("test#=x", 4)]
   [InlineData("test1~x", 5)]
   [InlineData("t@est1~x", 1)]
   [InlineData(" test ~x", 6)]
   public void FilteringError_WhenInvalidCharacterDetected_ShouldReturnErrorWithCharacterIndex(string filter, int index)
   {
      // arrange
      var dataSource = new List<TestClass>().AsQueryable();
      var gq = new GridifyQuery() { Filter = filter };

      // act
      var act = () => dataSource.ApplyFiltering(gq);

      // assert
      var exception = Assert.Throws<GridifyFilteringException>(act);
      _testOutputHelper.WriteLine(exception.Message);
      Assert.Contains($"at index {index.ToString()}", exception.Message);
      Assert.Contains("bad character input", exception.Message);
   }
}
