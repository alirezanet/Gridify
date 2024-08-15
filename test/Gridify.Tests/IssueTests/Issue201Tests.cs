using System.Collections.Generic;
using Xunit;
using Xunit.Abstractions;

namespace Gridify.Tests.IssueTests;

public class Issue201Tests(ITestOutputHelper output)
{

   [Fact]
   public void DictionaryIndexer_ShouldUpdateTheIndexIfIsChanged()
   {
      // Act
      var query = new QueryBuilder<TestModel>()
         .AddMap<long>("prop", (field, key) => field.Prop[key])
         .AddCondition("prop[44] = 2024")
         .AddCondition("prop[45] = 2025")
         .BuildFilteringExpression();


      // Assert
      output.WriteLine(query.ToString());
      Assert.Contains("field.Prop.get_Item(44) == 2024", query.ToString());
      Assert.Contains("field.Prop.get_Item(45) == 2025", query.ToString());
   }


   public class TestModel
   {
      public Dictionary<long, long> Prop { get; set; } = new();
   }
}
