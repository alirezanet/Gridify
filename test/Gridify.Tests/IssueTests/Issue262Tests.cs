using Gridify.Syntax;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Xunit;

namespace Gridify.Tests.IssueTests;

public class TestOperator : IGridifyOperator
{
   public string GetOperator()
   {
      return "#Test";
   }

   public Expression<OperatorParameter> OperatorHandler()
   {
      return (prop, value) => value.ToString().Equals("Test Operator");
   }
}

public class Issue262Tests
{
   [Fact]
   public async Task RegisterOperator_ShouldBeAllowedToBeRegisteredMultipleTimes_InDifferentThreads()
   {
      var tasks = Enumerable.Range(1, Environment.ProcessorCount)
         .Select(e =>
         {
            return Task.Run(() =>
            {
               for (int i = 0; i < 10_000; ++i)
               {
                  GridifyGlobalConfiguration.CustomOperators.Register<TestOperator>();
               }
            });
         });
      await Task.WhenAll(tasks);


      var testOperator = new TestOperator();
      var registeredOperators = GridifyGlobalConfiguration.CustomOperators
         .Operators
         .Where(o => o.GetOperator() == testOperator.GetOperator());

      Assert.Single(registeredOperators);
   }
}
