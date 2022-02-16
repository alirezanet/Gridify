using System;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using Gridify;
using Gridify.Syntax;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EntityFrameworkIntegrationTests.cs;

public class GridifyEntityFrameworkTests
{
   private readonly MyDbContext _dbContext;

   public GridifyEntityFrameworkTests()
   {
      _dbContext = new MyDbContext();
   }

   // issue #24,
   // https://github.com/alirezanet/Gridify/issues/24
   [Fact]
   public void ApplyFiltering_GeneratedSqlShouldNotCreateParameterizedQuery_WhenCompatibilityLayerIsDisable_SqlServerProvider()
   {
      var actual = _dbContext.Users.ApplyFiltering("name = vahid").ToQueryString();
      var expected = _dbContext.Users.Where(q => q.Name == "vahid").ToQueryString();

      Assert.StartsWith("SELECT [u].[Id]", expected);
      Assert.StartsWith("SELECT [u].[Id]", actual);
   }

   // issue #24,
   // https://github.com/alirezanet/Gridify/issues/24
   [Fact]
   public void ApplyFiltering_GeneratedSqlShouldCreateParameterizedQuery_SqlServerProvider()
   {
      GridifyGlobalConfiguration.EnableEntityFrameworkCompatibilityLayer();

      var name = "vahid";
      var expected = _dbContext.Users.Where(q => q.Name == name).ToQueryString();

      var actual = _dbContext.Users.ApplyFiltering("name = vahid").ToQueryString();

      Assert.StartsWith("DECLARE @__Value", actual);
      Assert.StartsWith("DECLARE @__name", expected);
   }


   // issue #27 ef core sqlServer feedback, and issue #24
   // https://github.com/alirezanet/Gridify/issues/27#issuecomment-929221457
   [Fact]
   public void ApplyFiltering_GreaterThanBetweenTwoStringsInEF_SqlServerProvider_EnableCompatibilityLayer()
   {
      GridifyGlobalConfiguration.EnableEntityFrameworkCompatibilityLayer();
      var sb = new StringBuilder();
      sb.AppendLine("DECLARE @__Value_0 nvarchar(4000) = N'h';");
      sb.AppendLine("SELECT [u].[Id], [u].[CreateDate], [u].[FkGuid], [u].[Name]");
      sb.AppendLine("FROM [Users] AS [u]");
      sb.AppendLine("WHERE [u].[Name] > @__Value_0");

      var actual = _dbContext.Users.ApplyFiltering("name > h").ToQueryString();
      Assert.True(string.Compare(
         sb.ToString(),
         actual,
         CultureInfo.CurrentCulture,
         CompareOptions.IgnoreCase | CompareOptions.IgnoreSymbols) == 0);
   }

   // issue #27 ef core sqlServer feedback
   // https://github.com/alirezanet/Gridify/issues/27#issuecomment-929221457dd
   [Fact]
   public void ApplyFiltering_GreaterThanBetweenTwoStringsInEF_SqlServerProvider_ShouldThrowErrorWhenCompatibilityLayerIsDisabled()
   {
      GridifyGlobalConfiguration.DisableEntityFrameworkCompatibilityLayer();
      Assert.Throws<InvalidOperationException>(() => _dbContext.Users.ApplyFiltering("name > h").ToQueryString());
   }


   // Support EF.Functions.FreeText #42
   // https://github.com/alirezanet/Gridify/issues/42
   [Fact]
   public void ApplyFiltering_EFFunction_FreeTextOperator()
   {
      GridifyGlobalConfiguration.CustomOperators.Register(new FreeTextOperator());

      // Arrange
      var expected = _dbContext.Users.Where(q => EF.Functions.FreeText(q.Name, "test")).ToQueryString();

      // Act
      var actual = _dbContext.Users.ApplyFiltering("name #=* test").ToQueryString();

      // Assert
      Assert.Equal(expected, actual);
   }

   internal class FreeTextOperator : IGridifyOperator
   {
      public string GetOperator() => "#=*";

      public Expression<OperatorParameter> OperatorHandler()
      {
         return (prop, value) => EF.Functions.FreeText(prop, value.ToString());
      }
   }

   [Fact]
   public void ApplyFiltering_ShadowProperty()
   {
      var gm = new GridifyMapper<User>()
         .AddMap("x", (User q) => EF.Property<string>(q, "shadow1"));

      var expected = _dbContext.Users.Where(q => EF.Property<string>(q, "shadow1") == "test").ToQueryString();
      var actual = _dbContext.Users.ApplyFiltering("x = test", gm).ToQueryString();

      Assert.Equal(expected, actual);
   }
}
