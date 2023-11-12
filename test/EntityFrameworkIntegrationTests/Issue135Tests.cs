using System;
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
   public void CustomOperator_WithGuid_ShouldNotThrowException()
   {
      // arrange
      GridifyGlobalConfiguration.CustomOperators.Register<GuidInOperator>();
      var targetGuid = Guid.NewGuid();

      var repo = new List<TestTask>()
      {
         new() { GuidState = targetGuid },
         new() { GuidState = Guid.NewGuid() },
         new() { GuidState = Guid.NewGuid() },
         new() { GuidState = Guid.NewGuid() },
      }.AsQueryable();

      var ids = new List<Guid> { Guid.NewGuid(), targetGuid, Guid.NewGuid() };
      var expected = repo.Where(x => ids.Contains(x.GuidState)).ToList();

      // act
      var actual = new QueryBuilder<TestTask>()
         .AddMap("gstate", q => q.GuidState,
            value => value.Split(";", StringSplitOptions.RemoveEmptyEntries).Select(Guid.Parse).ToList())
         .AddCondition($"gstate #In {Guid.NewGuid()};{Guid.NewGuid()};{targetGuid}")
         .Build(repo)
         .ToList();

      // assert
      Assert.Equal(expected.Count, actual.Count);
      Assert.Equal(expected.First().GuidState, actual.First().GuidState);

      // clean-up
      GridifyGlobalConfiguration.CustomOperators.Remove<GuidInOperator>();
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

   [Fact]
   public void CustomOperator_WithoutMapperConvertor_ShouldGenerateExactSameSqlQuery()
   {
      // arrange
      GridifyGlobalConfiguration.EnableEntityFrameworkCompatibilityLayer();
      GridifyGlobalConfiguration.CustomOperators.Register<StringInOperator>();

      // ReSharper disable once InconsistentNaming (needed for the test)
      var Value = new List<string> { "2", "4", "6" };
      var expected = _dbContext.Users.Where(q => Value.Contains(q.Name)).ToQueryString();

      // act
      var actual = new QueryBuilder<User>()
         .AddCondition("name #In2 2;4;6")
         .Build(_dbContext.Users)
         .ToQueryString();

      // assert
      Assert.Equal(expected, actual);

      GridifyGlobalConfiguration.CustomOperators.Remove<StringInOperator>();
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


class StringInOperator : IGridifyOperator
{
   public string GetOperator()
   {
      return "#In";
   }

   public Expression<OperatorParameter> OperatorHandler()
   {
      return (prop, value) => value.ToString()
         .Split(";", StringSplitOptions.RemoveEmptyEntries)
         .Contains(prop.ToString());
   }
}

class GuidInOperator : IGridifyOperator
{
   public string GetOperator()
   {
      return "#In";
   }
   public Expression<OperatorParameter> OperatorHandler()
   {
      return (prop, value) => ((List<Guid>)value).Contains((Guid)prop);
   }
}

internal class TestTask
{
   public int State { get; set; }
   public Guid GuidState { get; set; }
}
