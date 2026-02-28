using Xunit;

namespace Gridify.Tests.IssueTests;

public class Issue282Tests
{
   [Fact]
   public void IsValid_WhenFilterIsNotComplete_ShouldReturnFalse()
   {
      // arrange
      var mapping = new GridifyMapper<Issue282Entity>()
         .AddMap("firstName", q => q.FirstName)
         .AddMap("lastName", q => q.LastName);

      var query = new GridifyQuery
      {
         Filter = "firstName"
      };

      // act
      var result = query.IsValid(mapping);

      // assert
      Assert.False(result);
   }
}

class Issue282Entity
{
   public string FirstName { get; set; } = string.Empty;
   public string LastName { get; set; } = string.Empty;
}
