using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Gridify.Tests
{
   public class QueryBuilderShould
   {
      private readonly List<TestClass> _fakeRepository;
      private int _testRecordsCount => _fakeRepository.Count;

      public QueryBuilderShould()
      {
         _fakeRepository = new List<TestClass>(GridifyExtensionsShould.GetSampleData());
      }

      [Fact]
      public void Builder_Should_Return_All_Records_When_No_Filters_Are_Specified()
      {
         var builder = new QueryBuilder<TestClass>();

         var collection = builder.Build(_fakeRepository);
         var query = builder.Build(_fakeRepository.AsQueryable());
         var (count1, _) = builder.BuildWithPaging(_fakeRepository.AsQueryable());
         var (count2, _) = builder.BuildWithPaging(_fakeRepository);
         var (count3, _) = builder.BuildWithQueryablePaging(_fakeRepository.AsQueryable());

         Assert.Equal(_testRecordsCount, collection.Count());
         Assert.Equal(_testRecordsCount, query.Count());
         Assert.Equal(_testRecordsCount, count1);
         Assert.Equal(_testRecordsCount, count2);
         Assert.Equal(_testRecordsCount, count3);
      }

      [Fact] // issue #38
      public void Evaluator_Should_Check_All_Conditions_Without_And()
      {
         var builder = new QueryBuilder<TestClass>()
            .AddCondition("name =Ali, id < 6")
            .AddCondition("name =Sara, Id > 6");

         // using CollectionEvaluator
         var evaluator = builder.BuildCollectionEvaluator();
         Assert.True(evaluator(_fakeRepository));

         // using QueryableEvaluator
         var queryableEvaluator = builder.BuildQueryableEvaluator();
         Assert.True(queryableEvaluator(_fakeRepository.AsQueryable()));

         // Using Evaluate method (collection)
         var isValid = builder.Evaluate(_fakeRepository);
         Assert.True(isValid);

         // Using Evaluate method (queryable)
         var isQueryValid = builder.Evaluate(_fakeRepository.AsQueryable());
         Assert.True(isQueryValid);
      }

      [Fact]
      public void Build()
      {
         var builder = new QueryBuilder<TestClass>()
            .AddCondition("name =*al")
            .AddOrderBy("name");

         var compiled = builder.Build();
         var result = compiled(_fakeRepository.AsQueryable());
         Assert.True(result.Any());

      }

   }
}
