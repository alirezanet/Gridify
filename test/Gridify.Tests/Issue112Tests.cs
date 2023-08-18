using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Gridify.Syntax;
using Xunit;

namespace Gridify.Tests;

public class Issue112Tests
{
   [Fact]
   public void ApplyFiltering_OnFlagsUsingCustomOperator_ShouldParseTheExpressionWithoutError()
   {
      // arrange
      GridifyGlobalConfiguration.CustomOperators.Register(new BitwiseAndOperator());

      var lst = new List<AbilityComponents>()
      {
         new() {Prop = ComponentOperationFlags.Opt1 },
         new() {Prop = ComponentOperationFlags.Opt2 },
         new() {Prop = ComponentOperationFlags.Opt1 | ComponentOperationFlags.Opt2 }
      };
      var expected = lst.Where(l => ((uint)l.Prop & 1) != 0);

      // act
      var actual = lst.AsQueryable().ApplyFiltering("Prop #* 1");

      // assert
      Assert.NotEmpty(actual);
      Assert.Equal(expected, actual);
   }

   public class AbilityComponents
   {
      public ComponentOperationFlags Prop { get; set; }
   }

   [Flags]
   public enum ComponentOperationFlags : uint
   {
      None = 0,
      Opt1 = 1,
      Opt2 = 2,
      Opt3 = 4
   }

   public class BitwiseAndOperator : IGridifyOperator
   {
      public string GetOperator() => "#*";

      public Expression<OperatorParameter> OperatorHandler()
      {
         return (prop, value) => ((uint)prop & (uint)value) != 0;
      }
   }
}
