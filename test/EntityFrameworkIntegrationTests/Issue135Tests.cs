using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using EntityFrameworkIntegrationTests.cs;
using Gridify.Syntax;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Gridify.Tests.IssueTests;

public class Issue135Tests
{
   private readonly MyDbContext _dbContext = new();

   [Fact]
   public void CustomOperator_WithAnyTypePassedAsValue_ShouldNotThrowException()
   {
      // arrange
      GridifyGlobalConfiguration.CustomOperators.Register<TestInOperator>();
      var repo = new List<TestTask>
      {
         new() { State = 1 },
         new() { State = 2 },
         new() { State = 3 },
         new() { State = 4 }
      }.AsQueryable();
      var ids = new List<int> { 2, 4, 6 };
      var expected = repo.Where(x => ids.Contains(x.State)).ToList();

      // act
      var actual = new QueryBuilder<TestTask>()
         .AddMap("state", q => q.State, value => value.Split(";").Select(int.Parse).ToList())
         .AddCondition("state#In2;4;6")
         .Build(repo)
         .ToList();

      // assert
      Assert.Equal(expected.Count, actual.Count);
      Assert.Equal(expected.First().State, actual.First().State);

      // clean-up
      GridifyGlobalConfiguration.CustomOperators.Remove<TestInOperator>();
   }

   [Fact]
   public void CustomOperator_ShouldGenerateExactSameSqlQuery()
   {
      // arrange
      GridifyGlobalConfiguration.EnableEntityFrameworkCompatibilityLayer();
      GridifyGlobalConfiguration.CustomOperators.Register<TestInOperator>();

      // ReSharper disable once InconsistentNaming (needed for the test)
      var Value = new List<int> { 2, 4, 6 };
      var expected = _dbContext.Users.Where(q => Value.Contains(q.Id)).ToQueryString();

      // act
      var actual = new QueryBuilder<User>()
         .AddMap("state", q => q.Id, value => value.Split(";").Select(int.Parse).ToList())
         .AddCondition("state#In2;4;6")
         .Build(_dbContext.Users)
         .ToQueryString();

      // assert
      Assert.Equal(expected, actual);

      GridifyGlobalConfiguration.CustomOperators.Remove<TestInOperator>();
   }
}

internal class TestInOperator : IGridifyOperator
{
   public string GetOperator()
   {
      return "#In";
   }

   public Expression<OperatorParameter> OperatorHandler()
   {
      return (prop, value) => ((List<int>)value).Contains((int)prop);
   }
}

internal class TestTask
{
   public int State { get; set; }
}
