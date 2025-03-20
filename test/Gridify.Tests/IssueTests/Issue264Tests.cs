using System.Linq;
using System.Linq.Expressions;
using AutoBogus;
using Gridify.Syntax;
using Xunit;

namespace Gridify.Tests.IssueTests;

public class Issue264Tests
{
   public class LongOperator : IGridifyOperator
   {
      public string GetOperator()
      {
         return "#THIS»IS»A»VERY»LONG»OPERATOR»NAME»THAT»SHOULD»BE»ALLOWED»TO»BE»REGISTERED";
      }

      public Expression<OperatorParameter> OperatorHandler()
      {
         return (prop, value) => value.ToString()!.Equals(prop.ToString());
      }
   }

   public class ShortOperator : IGridifyOperator
   {
      public string GetOperator()
      {
         return "#S";
      }

      public Expression<OperatorParameter> OperatorHandler()
      {
         return (prop, value) => value.ToString()!.Equals(prop.ToString());
      }
   }

   [Fact]
   public void Lexer_ShouldAllowLongOperatorToExist()
   {
      GridifyGlobalConfiguration.CustomOperators.Register<LongOperator>();
      GridifyGlobalConfiguration.CustomOperators.Register<ShortOperator>();

      var fakeList = AutoFaker.Generate<TestClass>(10);
      fakeList.Add(new TestClass() { Id = 2 });

      var result = fakeList.AsQueryable().ApplyFiltering("id#S2").Distinct().ToList();
      var first = result.First();

      Assert.Single(result);
      Assert.Equal(2, first.Id);
   }

   [Fact]
   public void Lexer_ShouldAllowLongOperatorToExist_ReverseOrder()
   {
      GridifyGlobalConfiguration.CustomOperators.Register<ShortOperator>();
      GridifyGlobalConfiguration.CustomOperators.Register<LongOperator>();

      var fakeList = AutoFaker.Generate<TestClass>(10);
      fakeList.Add(new TestClass() { Id = 2 });

      var result = fakeList.AsQueryable().ApplyFiltering("id#S2").Distinct().ToList();
      var first = result.First();

      Assert.Single(result);
      Assert.Equal(2, first.Id);
   }
}
