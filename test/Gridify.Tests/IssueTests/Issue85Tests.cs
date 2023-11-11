using System;
using System.Linq.Expressions;
using Xunit;

namespace Gridify.Tests.IssueTests;

public class Issue85Tests
{
   [Fact]
   public void MultipleNullOrEmptyFilter_WithoutNullKeyword_ShouldWork()
   {
      // Arrange
      var qb = new QueryBuilder<TestClass>()
         .AddCondition("name=,Tag=");

      Expression<Func<TestClass, bool>> expected =
         __TestClass => (string.IsNullOrEmpty(__TestClass.Name) && string.IsNullOrEmpty(__TestClass.Tag));

      // Act
      var actual = qb.BuildFilteringExpression();

      // Assert
      Assert.Equal(expected.ToString(), actual.ToString());
   }

   [Fact]
   public void MultipleNullOrEmptyFilterWithParenthesis_WithoutNullKeyword_ShouldWork()
   {
      // Arrange
      var qb = new QueryBuilder<TestClass>()
         .AddCondition("((name=)|(Tag=)),Id=123");

      Expression<Func<TestClass, bool>> expected =
         __TestClass => (string.IsNullOrEmpty(__TestClass.Name) || string.IsNullOrEmpty(__TestClass.Tag)) && __TestClass.Id == 123;

      // Act
      var actual = qb.BuildFilteringExpression();

      // Assert
      Assert.Equal(expected.ToString(), actual.ToString());
   }
}
