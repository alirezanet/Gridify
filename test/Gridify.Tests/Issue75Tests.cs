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


   [Fact]
   public void NestedFiltering_OnChildElement_ShouldWorkOnExecution()
   {
      // Arrange
      var testList = new List<Root>()
      {
         new() { Child1 = new Child1 { Child2List = new List<Child2>() { new() { Name = "name" } } } },
         new() { Child1 = new Child1 { Child2List = new List<Child2>() { new() { Name = "name2" } } } },
      };
      ;

      var mapping = new GridifyMapper<Root>()
         .AddMap("x1", x => x.Child1.Child2List.Select(c => c.Name));

      // ReSharper disable once ConditionIsAlwaysTrueOrFalse
      Expression<Func<Root, bool>> expectedExpression = x => (x.Child1.Child2List != null) && (x.Child1.Child2List.Any(c => c.Name == "name"));
      var expected = testList.AsQueryable().Where(expectedExpression).ToList();

      // Act
      var actual = testList.AsQueryable().ApplyFiltering("x1=name", mapping).ToList();

      // Assert
      Assert.Equal(expected, actual);
      Assert.NotEmpty(actual);
   }

   [Fact]
   public void NestedFiltering_OnThirdLevelChildElement_ShouldWorkOnExecution()
   {
      // Arrange
      var testList = new List<Root>()
      {
         new() { Child1 = new Child1 { Child2 = new Child2() { Child3List = new List<Child3>() { new() { Name = "name" } } } } },
         new() { Child1 = new Child1 { Child2 = new Child2() { Child3List = new List<Child3>() { new() { Name = "name2" } } } } }
      };


      var mapping = new GridifyMapper<Root>()
         .AddMap("x1", x => x.Child1.Child2.Child3List.Select(c => c.Name));

      // ReSharper disable once ConditionIsAlwaysTrueOrFalse
      Expression<Func<Root, bool>> expectedExpression =
         x => (x.Child1.Child2.Child3List != null) && (x.Child1.Child2.Child3List.Any(c => c.Name == "name"));
      var expected = testList.AsQueryable().Where(expectedExpression).ToList();

      // Act
      var actual = testList.AsQueryable().ApplyFiltering("x1=name", mapping).ToList();

      // Assert
      Assert.Equal(expected, actual);
      Assert.NotEmpty(actual);
   }
}

public class Root
{
   public Child1 Child1 { get; set; }
   public IEnumerable<Child2> Child2List { get; set; }
}

public class Child1
{
   public Child2 Child2 { get; set; }
   public IEnumerable<Child2> Child2List { get; set; }
}

public class Child2
{
   public string Name { get; set; }
   public IEnumerable<Child3> Child3List { get; set; }
}

public class Child3
{
   public string Name { get; set; }
}
