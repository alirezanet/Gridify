using System.Collections.Generic;
using System.Linq;
using Gridify;
using Xunit;

namespace Gridify.Tests;

public class QueryBuilderSelectTests
{
   private static List<TestClass> SampleData() =>
   [
      new(1, "Alice", null),
      new(2, "Bob", null),
      new(3, "Carol", null)
   ];

   [Fact]
   public void AddSelect_BuildSelect_ReturnsProjectedQueryable()
   {
      var qb = new QueryBuilder<TestClass>().AddSelect("name");
      var data = SampleData().AsQueryable();
      var result = qb.BuildSelect(data).ToList();

      Assert.Equal(3, result.Count);
      Assert.NotNull(result[0].GetType().GetProperty("name"));
   }

   [Fact]
   public void AddSelect_DoesNotAffectTypedBuild()
   {
      var qb = new QueryBuilder<TestClass>().AddSelect("name");
      var data = SampleData().AsQueryable();
      var typedResult = qb.Build(data).ToList();

      Assert.All(typedResult, x => Assert.IsType<TestClass>(x));
   }

   [Fact]
   public void BuildSelectWithPaging_PaginatesAndProjects()
   {
      var qb = new QueryBuilder<TestClass>()
         .AddCondition("id>0")
         .ConfigurePaging(0, 2)
         .AddSelect("name");
      var data = SampleData().AsQueryable();
      Paging<object> result = qb.BuildSelectWithPaging(data);

      Assert.Equal(3, result.Count);
      Assert.Equal(2, result.Data.Count());
   }

   [Fact]
   public void BuildSelect_FuncOverload_Works()
   {
      var qb = new QueryBuilder<TestClass>().AddSelect("name");
      var data = SampleData().AsQueryable();
      var fn = qb.BuildSelect();
      var result = fn(data).ToList();
      Assert.Equal(3, result.Count);
   }

   [Fact]
   public void AddQuery_WithSelect_ForwardsToBuilder()
   {
      // QueryBuilder treats Page as 0-indexed (Skip(page * pageSize)). Use Page = 0 to fetch the first page.
      var gq = new GridifyQuery { Select = "name", Page = 0, PageSize = 10 };
      var qb = new QueryBuilder<TestClass>().AddQuery(gq);
      var data = SampleData().AsQueryable();
      var result = qb.BuildSelect(data).ToList();

      Assert.Equal(3, result.Count);
      Assert.NotNull(result[0].GetType().GetProperty("name"));
   }
}
