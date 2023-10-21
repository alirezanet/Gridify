using System.Collections.Generic;
using System.Linq;
using Gridify.Syntax;
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
      Assert.Contains($", at index {index.ToString()}", exception.Message);
      Assert.Contains("bad character input", exception.Message);
   }

   [Theory]
   [InlineData("test=(1234)", 5, SyntaxKind.OpenParenthesisToken, SyntaxKind.ValueToken)]
   [InlineData("test(=123", 4, SyntaxKind.OpenParenthesisToken, SyntaxKind.Operator)]
   [InlineData("(test=2", 7, SyntaxKind.End, SyntaxKind.CloseParenthesis)]
   [InlineData("test=,", 6, SyntaxKind.End, SyntaxKind.FieldToken)] // test=, is valid if there is another expression after <AND> operator
   [InlineData("test=2,2", 7, SyntaxKind.BadToken, SyntaxKind.FieldToken)]
   public void FilteringError_WhenInvalidTokenDetected_ShouldReturnErrorWithExpectedToken(
      string filter,
      int index,
      SyntaxKind unexpectedToken,
      SyntaxKind expectedToken)
   {
      // arrange
      var dataSource = new List<TestClass>().AsQueryable();
      var gq = new GridifyQuery() { Filter = filter };

      // act
      var act = () => dataSource.ApplyFiltering(gq);

      // assert
      var exception = Assert.Throws<GridifyFilteringException>(act);
      _testOutputHelper.WriteLine(exception.Message);
      Assert.Contains($"Unexpected token <{unexpectedToken}> at index {index}, expected <{expectedToken}>", exception.Message);
   }
}
