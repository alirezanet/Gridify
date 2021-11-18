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

      // [Fact]
      // public void AddCondition_Should_Filter_By_Property_Name()
      // {
      //    var builder = new QueryBuilder<TestClass>()
      //       .AddCondition("Name =* a");
      //
      //
      // }

   }
}
