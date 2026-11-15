using System.Linq;
using Xunit;

namespace Gridify.Tests.IssueTests;

public class Issue353Tests
{
   [Theory]
   [InlineData("$12")]
   [InlineData("^a")]
   [InlineData("=a")]
   [InlineData("!=a")]
   [InlineData(">5")]
   [InlineData("<5")]
   [InlineData(">=5")]
   [InlineData("<=5")]
   [InlineData("=*a")]
   [InlineData("!*a")]
   [InlineData("!^a")]
   [InlineData("!$a")]
   public void Filtering_ValueStartingWithOperatorCharacter_ShouldNotThrow(string value)
   {
      // arrange
      var rows = new[] { new TestClass { Name = value }, new TestClass { Name = "other" } }.AsQueryable();

      // act
      var actualContains = rows.ApplyFiltering($"Name =* {value}").ToList();
      var actualContainsNoSpace = rows.ApplyFiltering($"Name=*{value}").ToList();
      var actualEquals = rows.ApplyFiltering($"Name = {value}").ToList();
      var actualStartsWith = rows.ApplyFiltering($"Name ^ {value}").ToList();

      // assert
      Assert.Single(actualContains);
      Assert.Equal(value, actualContains[0].Name);
      Assert.Single(actualContainsNoSpace);
      Assert.Equal(value, actualContainsNoSpace[0].Name);
      Assert.Single(actualEquals);
      Assert.Equal(value, actualEquals[0].Name);
      Assert.Single(actualStartsWith);
      Assert.Equal(value, actualStartsWith[0].Name);
   }

   [Fact]
   public void Filtering_ValueStartingWithOperatorAndCaseInsensitive_ShouldFilter()
   {
      // arrange
      var rows = new[] { new TestClass { Name = "$ABC" }, new TestClass { Name = "other" } }.AsQueryable();

      // act
      var actual = rows.ApplyFiltering("Name =* $abc/i").ToList();

      // assert
      Assert.Single(actual);
      Assert.Equal("$ABC", actual[0].Name);
   }
}
