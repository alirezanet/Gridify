using System;
using System.Linq.Expressions;
using Xunit;

namespace Gridify.Tests;

public class Issue101Tests
{
   [Fact]
   private void Filtering_WhenEscapedBackslashIsUsedAtTheEnd_ShouldNotEscapeMoreCharacters()
   {
      // Arrange
      var qb = new QueryBuilder<TestClass>()
         .AddCondition(@"name=*\\");

      Expression<Func<TestClass, bool>> expectedExpression = (__TestClass) => __TestClass.Name!.Contains("\\");
      var expected = expectedExpression.ToString();

      // Act
      var actual = qb.BuildFilteringExpression().ToString();

      // Assert
      Assert.Equal(expected, actual);
   }

   [Fact]
   private void Filtering_WhenEscapedBackslashIsUsedAtTheEndWithParenthesis_ShouldNotEscapeMoreCharacters()
   {
      // Arrange
      var qb = new QueryBuilder<TestClass>()
         .AddCondition(@"(name=*\\)");

      Expression<Func<TestClass, bool>> expectedExpression = (__TestClass) => __TestClass.Name!.Contains("\\");
      var expected = expectedExpression.ToString();

      // Act
      var actual = qb.BuildFilteringExpression().ToString();

      // Assert
      Assert.Equal(expected, actual);
   }
}
