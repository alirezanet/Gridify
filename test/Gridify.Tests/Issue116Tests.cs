using System;
using System.Linq;
using System.Linq.Expressions;
using AutoBogus;
using Gridify.Syntax;
using Xunit;

namespace Gridify.Tests;

public class Issue116Tests
{
   [Fact]
   public void CustomOperator_WhenCastingTypes_ShouldNotThrowException()
   {
      // arrange
      GridifyGlobalConfiguration.CustomOperators.Register(new InOperator());
      var fakeList = AutoFaker.Generate<TestClass>(10);
      fakeList.Add(new TestClass() { Id = 2 });

      // act
      var result = fakeList.AsQueryable().ApplyFiltering("id#in2;3").Distinct().ToList();

      // assert
      Assert.Single(result);
   }

   private class InOperator : IGridifyOperator
   {
      public string GetOperator() => "#in";
      public Expression<OperatorParameter> OperatorHandler() =>
         (prop, value) => ((string)value).Split(';', StringSplitOptions.None).Contains(prop.ToString());
   }

}
