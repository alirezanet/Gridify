using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using Xunit;

namespace Gridify.Tests;

// Related to Issue #75
public class Issue75Tests
{
   [Fact]
   public void NestedFiltering_OnChildElement_ShouldNotThrowException()
   {
      // Arrange
      var mapping = new GridifyMapper<Root>()
         .AddMap("Child2Name", x => x.Child2List.Select(c => c.Name)) // Works fine
         .AddMap("Child2NameNested", x => x.Child1.Child2List.Select(c => c.Name)); // Throws exception

      // Act
      var exp = new GridifyQuery { Filter = "Child2NameNested=name" }.GetFilteringExpression(mapping);

      // Assert
      Assert.NotNull(exp);
   }

   [Fact]
   public void NestedFiltering_OnChildElement_ShouldReturnAValidExpression()
   {
      // Arrange
      var mapping = new GridifyMapper<Root>()
          .AddMap("x1", x => x.Child1.Child2List.Select(c => c.Name));

      // ReSharper disable once ConditionIsAlwaysTrueOrFalse
      Expression<Func<Root, bool>> expectedExpression = x => (x.Child1.Child2List != null) && (x.Child1.Child2List.Any(c => c.Name == "name"));
      var expected = expectedExpression.ToString();

      // Act
      var actual = new GridifyQuery { Filter = "x1=name" }.GetFilteringExpression(mapping).ToString();

      // Assert
      // "x => ((x.Child1.Child2List != null) AndAlso x.Child1.Child2List.Any(c => (c.Name == \"name\")))";
      Assert.Equal(expected, actual);

   }

}



public class Root
{
   public Child1 Child1 { get; set; }
   public IEnumerable<Child2> Child2List { get; set; }
}

public class Child1
{
   public IEnumerable<Child2> Child2List { get; set; }
}

public class Child2
{
   public string Name { get; set; }
}
