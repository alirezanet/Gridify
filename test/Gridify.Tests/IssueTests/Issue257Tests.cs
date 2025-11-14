using System.Collections.Generic;
using Gridify;
using Xunit;

public class Issue257Tests
{
   [Fact]
   public void AddMap_WithoutCollectionNullChecks_ShouldNotThrow()
   {
      var query = new GridifyQuery()
      {
         Filter = "MetaDataDefined[CreatedOn]=false"
      };
      var mapper = new GridifyMapper<Person>();
      mapper.Configuration.DisableCollectionNullChecks = false; //true works around the issue
      mapper.AddMap("MetaDataDefined", (person, key) => Utils.IsDefined(person.MetaData, key));
      query.GetFilteringExpression(mapper);
   }
   class Person
   {
      public Dictionary<string, string> MetaData { get; set; }
   }

   class Utils
   {
      public static bool IsDefined(Dictionary<string, string> dict, string key)
      {
         return dict.TryGetValue(key, out _);
      }
   }
}
