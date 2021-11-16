using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Gridify.Tests
{
   // issue #36 - https://github.com/alirezanet/Gridify/issues/36
   public class Issue36Tests
   {
      [Fact]
      private void UserReportTest1()
      {
         List<Level1> level1List = new List<Level1>();
         level1List.Add(new Level1());

         var gridifyMapper = new GridifyMapper<Level1>().GenerateMappings()
            .AddMap("level4_property1", (l1, index) => l1.Level2List.Select(x => x.Level3.Level4List[index].Property1));

         var gridifyQuery = new GridifyQuery() { Filter = "level4_property1[0] > 5" };
         var query = gridifyQuery.GetFilteringExpression(gridifyMapper);
         var expression = query.Compile();

         var actual = level1List.Where(expression).ToList();

         Assert.Single(actual);
         Assert.True(actual.Any());
      }

      public class Level1
      {
         public string Name { get; set; }

         public List<Level2> Level2List = new List<Level2>()
         {
            new Level2()
            {
               Name = "1",
               Level3 = new Level3()
               {
                  Name = "2",
                  Level4List = new List<Level4>()
                  {
                     new Level4() { Name = "3", Property1 = 3, Property2 = 4 },
                     new Level4() { Name = "4", Property1 = 4, Property2 = 5 },
                     new Level4() { Name = "5", Property1 = 5, Property2 = 6 }
                  }
               }
            },

            new Level2()
            {
               Name = "6",
               Level3 = new Level3()
               {
                  Name = "7",
                  Level4List = new List<Level4>()
                  {
                     new Level4() { Name = "8", Property1 = 8, Property2 = 9 },
                     new Level4() { Name = "9", Property1 = 9, Property2 = 10 },
                     new Level4() { Name = "10", Property1 = 10, Property2 = 11 }
                  }
               }
            },
         };
      }

      public class Level2
      {
         public string Name { get; set; }
         public Level3 Level3 = new Level3();
      }

      public class Level3
      {
         public string Name { get; set; }
         public List<Level4> Level4List = new List<Level4>();
      }

      public class Level4
      {
         public string Name { get; set; }
         public double Property1 { get; set; }
         public double Property2 { get; set; }
      }
   }
}