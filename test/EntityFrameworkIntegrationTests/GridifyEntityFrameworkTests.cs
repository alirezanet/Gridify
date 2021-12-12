using System.Linq;
using Gridify;
using Xunit;

namespace EntityFrameworkIntegrationTests.cs;

public class GridifyEntityFrameworkTests : IClassFixture<DatabaseFixture>
{
   private readonly DatabaseFixture fixture;
   private MyDbContext _ctx => fixture._dbContext;

   public GridifyEntityFrameworkTests(DatabaseFixture fixture)
   {
      this.fixture = fixture;
   }
   [Fact]
   public void EntityFrameworkServiceProviderCachingShouldNotThrowException()
   {
      // System.ArgumentException: An item with the same key has already been added. Key: Param_0

      // arrange
      var gq = new GridifyQuery { Filter = "name=n1|name=n2" };

      _ctx.Users.Gridify(gq);
      _ctx.Users.Gridify(gq);

      //act
      var exception = Record.Exception(() => _ctx.Users.GridifyQueryable(gq));

      // assert
      Assert.Null(exception);
   }

   [Fact]
   public void GridifyQueryableDateTimeShouldNotThrowException()
   {
      // arrange
      var gq = new GridifyQuery { OrderBy = "CreateDate" };

      // act
      var exception = Record.Exception(() => _ctx.Users.GridifyQueryable(gq));

      // assert
      Assert.Null(exception);
   }

   // issue #27 ef core feedback
   // here is working without using the `EnableEntityFrameworkCompatibilityLayer`
   // because EF In-Memory provider can support the StringComparison parameter
   // but it doesn't work in other sql providers
   // https://github.com/alirezanet/Gridify/issues/27#issuecomment-929221457
   [Fact]
   public void ApplyFiltering_GreaterThanBetweenTwoStringsInEF()
   {
      GridifyGlobalConfiguration.EnableEntityFrameworkCompatibilityLayer();

      var actual = _ctx.Users.ApplyFiltering("name > h").ToList();
      var expected = _ctx.Users.Where(q => string.Compare(q.Name, "h") > 0).ToList();

      Assert.Equal(expected.Count, actual.Count);
      Assert.Equal(expected, actual);
      Assert.True(actual.Any());
   }

   [Fact]
   public void Builder_BuildEvaluator_Should_Correctly_Evaluate_All_Conditions()
   {
      var builder = new QueryBuilder<User>()
         .AddCondition("name=*a")
         .AddCondition("id>3");

      var evaluator = builder.BuildEvaluator();
      var actual = evaluator(_ctx.Users);

      Assert.True(actual);
      Assert.True(builder.Evaluate(_ctx.Users));

      builder.AddCondition("name=fakeName");
      Assert.False(builder.Evaluate(_ctx.Users));

   }

}