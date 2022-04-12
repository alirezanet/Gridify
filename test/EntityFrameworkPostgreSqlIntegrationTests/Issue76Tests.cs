using System.Linq.Expressions;
using System.Reflection.Metadata;
using EntityFrameworkIntegrationTests.cs;
using Gridify.Syntax;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Gridify.Tests;

public class Issue76Tests
{

   private readonly MyDbContext _dbContext;

   public Issue76Tests()
   {
      _dbContext = new MyDbContext();
   }

   [Fact]
   public void CustomOperator_EFJsonContains_ShouldGenerateCorrectExpression()
   {
      // Arrange
      GridifyGlobalConfiguration.CustomOperators.Register(new JsonContainsOperator());

      var gm = new GridifyMapper<Products>()
         .AddMap("u", q => q.Users);
      var expected = _dbContext.ProductsViews.Where(q => EF.Functions.JsonContains(q.Users, new[] { new { Id = 1 } })).ToQueryString();

      // Act
      var actual = _dbContext.ProductsViews.ApplyFiltering("u #= 1", gm).ToQueryString();

      // Assert
      Assert.Equal(expected, actual);
   }

}

public class JsonContainsOperator : IGridifyOperator
{
   public string GetOperator() => "#=";
   public Expression<OperatorParameter> OperatorHandler()
   {
      return (prop, value) => EF.Functions.JsonContains(prop, new[] { new { Id = int.Parse(value.ToString()!) } });
   }
}
